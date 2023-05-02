# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

from typing import Tuple
from unittest.mock import patch
from os.path import join, abspath
import pytest
import os
import uuid
import random
import json
import sys

sys.path.append(join(abspath(__file__ + "../../../../../"), "actions", "validate-models"))
from process import (
    looks_like_dtmi,
    get_process_context,
    get_immutable_msg,
    get_warning_msg,
    get_validation_msg,
    validate_model,
    handle_immutable_set,
    handle_added_set,
    process,
    fail,
    tense_map,
)


def get_random_id():
    return str(uuid.uuid4()).replace("-", "")


def get_random_dtdl():
    return f"dtmi/{get_random_id()}.json"


def test_fail():
    with pytest.raises(RuntimeError):
        fail()


@pytest.mark.parametrize(
    "immutable_error,added_error,has_error",
    [
        (False, False, False),
        (False, True, True),
        (True, True, True),
        (True, False, True),
    ],
)
@patch("process.handle_added_set")
@patch("process.handle_immutable_set")
def test_process(
    mock_immutable_set,
    mock_added_set,
    cleanup_env_context,
    immutable_error,
    added_error,
    has_error,
):
    mock_immutable_set.return_value = immutable_error
    mock_added_set.return_value = added_error
    generate_test_env()

    if has_error:
        with pytest.raises(RuntimeError):
            process()
    else:
        process()


@pytest.mark.parametrize(
    "file_action,kwargs,has_error",
    [
        ("modified", {"modified_count": 1, "dtdl_files": True}, True),
        ("removed", {"removed_count": 1, "dtdl_files": True}, True),
        ("renamed", {"renamed_count": 1, "dtdl_files": True}, True),
        ("added", {"added_count": 1, "dtdl_files": True}, False),
        ("modified", {"modified_count": 1, "dtdl_files": False}, False),
        ("removed", {"removed_count": 1, "dtdl_files": False}, False),
        ("renamed", {"renamed_count": 1, "dtdl_files": False}, False),
        ("added", {"added_count": 1, "dtdl_files": False}, False),
    ],
)
@patch("process.print")
def test_handle_immutable_set(
    mock_print, cleanup_env_context, file_action, kwargs, has_error
):
    generate_test_env(**kwargs)
    context = get_process_context()
    result = handle_immutable_set(context)

    # True indicates error
    assert result == has_error

    if file_action != "added":
        assert (
            mock_print.call_args_list[0].args[0]
            == f"::group::Process {file_action} files"
        )
        issue_indicator = mock_print.call_args_list[1].args[0]
        assert (
            issue_indicator.startswith("::error")
            if has_error
            else issue_indicator.startswith("::warning")
        )
        assert mock_print.call_args_list[2].args[0].startswith("::endgroup::")


@pytest.mark.parametrize(
    "file_action,kwargs,has_error",
    [
        ("modified", {"modified_count": 1, "dtdl_files": True}, False),
        ("added", {"added_count": 0, "dtdl_files": True}, False),
        ("added", {"added_count": 1, "dtdl_files": False}, False),
        ("added", {"added_count": 1, "dtdl_files": True}, False),
        ("added", {"added_count": 1, "dtdl_files": True}, True),
    ],
)
@patch("process.subprocess.run")
@patch("process.print")
def test_handle_added_set(
    mock_print, mock_run, cleanup_env_context, file_action, kwargs, has_error
):
    import subprocess

    return_code = 1 if has_error else 0
    target_stderr = get_random_id()
    target_version = get_random_id()
    sample_debug = (
        "ModelsRepositoryCommandLine/1.0.0-beta.9 "
        "ModelsRepositoryClient/1.0.0-preview.4+b2e34443be8995bbb96d42ad32f5c3b290eed97e "
        f"DTDLParser/{target_version}\n"
    )
    mock_run.side_effect = [
        subprocess.CompletedProcess(
            [], 0, stderr=sample_debug
        ),  # Assume dmr-client is installed for version check.
        subprocess.CompletedProcess([], return_code, stderr=target_stderr),
    ]

    generate_test_env(**kwargs)
    context = get_process_context()
    result = handle_added_set(context)

    # True indicates error
    assert result == has_error
    if file_action == "added" and kwargs["added_count"] > 0:
        assert (
            mock_print.call_args_list[0].args[0]
            == f"::group::Process {file_action} files"
        )
        parser_version_indicator = mock_print.call_args_list[1].args[0]
        assert parser_version_indicator == (
            f"::notice title=Digital Twin Parser::Using Digital Twin Parser v{target_version} for model validation."
        )
        issue_indicator = mock_print.call_args_list[2].args[0]

        if kwargs["dtdl_files"] is False:
            assert issue_indicator.startswith("::warning")
        elif has_error:
            assert issue_indicator.startswith("::error") and issue_indicator.endswith(
                target_stderr
            )
        else:
            assert issue_indicator.startswith(
                "Validation of"
            ) and issue_indicator.endswith("OK!")

        assert mock_print.call_args_list[3].args[0].startswith("::endgroup::")


@pytest.mark.parametrize(
    "added_count,modified_count,removed_count,renamed_count",
    [
        (
            random.randint(1, 20),
            random.randint(1, 20),
            random.randint(1, 20),
            random.randint(1, 20),
        ),
        (random.randint(1, 20), 0, 0, 0),
        (0, 0, 0, 0),
        (None, None, None, None),
    ],
)
def test_get_process_context(
    cleanup_env_context, added_count, modified_count, removed_count, renamed_count
):
    (
        target_pwd,
        added_values,
        modified_values,
        removed_values,
        renamed_values,
    ) = generate_test_env(
        added_count=added_count,
        modified_count=modified_count,
        removed_count=removed_count,
        renamed_count=renamed_count,
    )

    context = get_process_context()
    assert context["repository"] == target_pwd
    assert context.get("added") == added_values
    assert context["immutable"].get("modified") == modified_values
    assert context["immutable"].get("removed") == removed_values
    assert context["immutable"].get("renamed") == renamed_values


@patch("process.subprocess.run")
def test_validate_model(mock_run):
    import subprocess
    import shlex

    mock_run.return_value = subprocess.CompletedProcess([], 0)

    repo = get_random_id()
    file = get_random_id()

    validate_model(repo, file)
    assert mock_run.call_count == 1

    cmd = f"dmr-client validate --model-file {file} --repo {repo} --strict"
    cmd = shlex.split(cmd)

    # args passed to .run()
    assert mock_run.call_args[0][0] == cmd
    # kwargs passed to .run()
    assert mock_run.call_args[1] == {"stderr": -1, "text": True}


def test_looks_like_dtmi():
    assert looks_like_dtmi("dtmi/fake/one.json") is True
    assert looks_like_dtmi("dtmi/fake/one/two.json") is True

    assert looks_like_dtmi("fake/one.json") is False
    assert looks_like_dtmi("dtmi/fake/one/two") is False
    assert looks_like_dtmi("artifact.json") is False


def test_get_immutable_msg():
    file = "my/file"
    action = "removed"
    msg = get_immutable_msg(action, file)
    assert (
        msg
        == f"::error file={file},title=Model file {tense_map[action]} {file}::Public repository models are immutable and cannot be {action}."
    )


def test_get_warning_msg():
    file = "my/file"
    action = "modified"
    msg = get_warning_msg(action, file)
    assert (
        msg
        == f"::warning file={file},title=File {tense_map[action]} detected {file}::Please review intent for {file}."
    )


def test_get_validation_msg():
    file = "my/file"
    output = "abcde"
    msg = get_validation_msg(output, file)
    assert msg == f"::error file={file},title=Model validation failure {file}::{output}"


def generate_test_env(
    added_count=None,
    modified_count=None,
    removed_count=None,
    renamed_count=None,
    dtdl_files=False,
) -> Tuple:
    added_values = None
    modified_values = None
    removed_values = None
    renamed_values = None

    get_file_name = get_random_dtdl if dtdl_files else get_random_id

    if added_count is not None:
        added_values = []
        for _ in range(0, added_count):
            added_values.append(get_file_name())
        os.environ["added"] = json.dumps(added_values)
    if modified_count is not None:
        modified_values = []
        for _ in range(0, modified_count):
            modified_values.append(get_file_name())
        os.environ["modified"] = json.dumps(modified_values)
    if removed_count is not None:
        removed_values = []
        for _ in range(0, removed_count):
            removed_values.append(get_file_name())
        os.environ["removed"] = json.dumps(removed_values)
    if renamed_count is not None:
        renamed_values = []
        for _ in range(0, renamed_count):
            renamed_values.append(get_file_name())
        os.environ["renamed"] = json.dumps(renamed_values)

    target_pwd = get_random_id()
    os.environ["PWD"] = target_pwd

    return target_pwd, added_values, modified_values, removed_values, renamed_values


@pytest.fixture
def cleanup_env_context():
    yield

    if "added" in os.environ:
        del os.environ["added"]
    if "modified" in os.environ:
        del os.environ["modified"]
    if "removed" in os.environ:
        del os.environ["removed"]
    if "renamed" in os.environ:
        del os.environ["renamed"]
