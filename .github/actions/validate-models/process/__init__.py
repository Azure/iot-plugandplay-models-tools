# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

import subprocess
import shlex
import json
import os


immutable_key = "immutable"
added_key = "added"
modified_key = "modified"
renamed_key = "renamed"
removed_key = "removed"
repo_key = "repository"

tense_map = {
    modified_key: "modification",
    removed_key: "removal",
    renamed_key: "rename",
    added_key: "addition",
}


def process():
    has_error = False
    context = get_process_context()
    immutable_result = handle_immutable_set(context)
    added_result = handle_added_set(context)
    has_error = immutable_result or added_result

    if has_error:
        fail()


def handle_immutable_set(context: dict) -> bool:
    has_error = False
    # Immutable set
    for p in context[immutable_key]:
        print(f"::group::Process {p} files")
        for f in context[immutable_key][p]:
            if looks_like_dtmi(f):
                print(get_immutable_msg(p, f))
                has_error = True
            else:
                print(get_warning_msg(p, f))
        print("::endgroup::")

    return has_error


def handle_added_set(context: dict) -> bool:
    has_error = False
    # Added files which may require validation
    repo_dir = context[repo_key]
    if context.get(added_key):
        print(f"::group::Process {added_key} files")
        for f in context[added_key]:
            if looks_like_dtmi(f):
                result = validate_model(repo_dir, f)
                if result.returncode != 0:
                    has_error = True
                    print(get_validation_msg(result.stderr, f))
                else:
                    print(f"Validation of {f} OK!")
            else:
                print(get_warning_msg(added_key, f))
        print("::endgroup::")

    return has_error


def get_process_context() -> dict:
    added_files = os.getenv(added_key)
    modified_files = os.getenv(modified_key)
    removed_files = os.getenv(removed_key)
    renamed_files = os.getenv(renamed_key)
    repo = os.getenv("PWD")

    to_process = {immutable_key: {}}
    to_process[repo_key] = repo

    if added_files:
        added_files = json.loads(added_files)
        to_process[added_key] = added_files
    if modified_files:
        modified_files = json.loads(modified_files)
        to_process[immutable_key][modified_key] = modified_files
    if removed_files:
        removed_files = json.loads(removed_files)
        to_process[immutable_key][removed_key] = removed_files
    if renamed_files:
        renamed_files = json.loads(renamed_files)
        to_process[immutable_key][renamed_key] = renamed_files

    return to_process


def looks_like_dtmi(file: str) -> bool:
    # Simple checks to support wider range of files
    if file.startswith("dtmi/") and file.endswith(".json"):
        return True
    return False


def get_immutable_msg(action: str, file: str) -> str:
    return f"::error file={file},title=Model file {tense_map[action]} {file}::Public repository models are immutable and cannot be {action}."


def get_warning_msg(action: str, file: str) -> str:
    return f"::warning file={file},title=File {tense_map[action]} detected {file}::Please review intent."


def get_validation_msg(output: str, file: str) -> str:
    return f"::error file={file},title=Model validation failure {file}::{output}"


def validate_model(repo: str, file: str) -> subprocess.CompletedProcess:
    cmd = f"dmr-client validate --model-file {file} --repo {repo} --strict"
    cmd = shlex.split(cmd)
    return subprocess.run(cmd, stderr=subprocess.PIPE, text=True)


def fail(msg="Validation failure."):
    raise RuntimeError(msg)
