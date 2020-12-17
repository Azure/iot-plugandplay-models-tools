# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------
import pytest
import requests
import json
from azure.iot.modelsrepository import resolver

class SharedResolverTests(object):
    pass


@pytest.fixture
def dtmi():
    return "dtmi:com:somedomain:example:FakeDTDL;1"


@pytest.fixture
def fake_json():
    return {"some_json_property": "some_json_val"}


@pytest.mark.describe(".resolve() -- Remote URL endpoint")
class TestResolverURL(SharedResolverTests):
    @pytest.fixture
    def endpoint(self):
        return "https://somedomain.com/"

    @pytest.fixture
    def mock_http_get(self, mocker, fake_json):
        mock_http_get = mocker.patch.object(requests, "get")
        mock_response = mock_http_get.return_value
        mock_response.status_code = 200
        mock_response.json.return_value = fake_json
        return mock_http_get

    @pytest.mark.it("Performs an HTTP GET on a URL path to a .json file created from combining the endpoint and DTMI")
    @pytest.mark.parametrize("endpoint, dtmi, expected_url", [
        pytest.param("http://somedomain.com/", "dtmi:com:somedomain:example:FakeDTDL;1", "http://somedomain.com/dtmi/com/somedomain/example/fakedtdl-1.json", id="HTTP endpoint"),
        pytest.param("https://somedomain.com/", "dtmi:com:somedomain:example:FakeDTDL;1", "https://somedomain.com/dtmi/com/somedomain/example/fakedtdl-1.json", id="HTTPS endpoint"),
        pytest.param("http://somedomain.com", "dtmi:com:somedomain:example:FakeDTDL;1", "http://somedomain.com/dtmi/com/somedomain/example/fakedtdl-1.json", id="Endpoint with no trailing '/'"),
    ])
    def test_regular_url(self, mocker, mock_http_get, endpoint, dtmi, expected_url):
        resolver.resolve(dtmi, endpoint)

        assert mock_http_get.call_count == 1
        assert mock_http_get.call_args == mocker.call(expected_url)

    @pytest.mark.it("Performs an HTTP GET on a URL path to a .expanded.json file created from combining the endpoint and DTMI, if the optional 'expanded' arg is True")
    @pytest.mark.parametrize("endpoint, dtmi, expected_url", [
        pytest.param("http://somedomain.com/", "dtmi:com:somedomain:example:FakeDTDL;1", "http://somedomain.com/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="HTTP endpoint"),
        pytest.param("https://somedomain.com/", "dtmi:com:somedomain:example:FakeDTDL;1", "https://somedomain.com/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="HTTPS endpoint"),
        pytest.param("http://somedomain.com", "dtmi:com:somedomain:example:FakeDTDL;1", "http://somedomain.com/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="Endpoint with no trailing '/'"),
    ])
    def test_expanded_url(self, mocker, mock_http_get, endpoint, dtmi, expected_url):
        resolver.resolve(dtmi, endpoint, expanded=True)

        assert mock_http_get.call_count == 1
        assert mock_http_get.call_args == mocker.call(expected_url)

    @pytest.mark.it("Returns a dictionary mapping the provided DTMI to the result of the HTTP GET, if the GET is successful (200 response)")
    def test_returns_dict(self, mocker, mock_http_get, endpoint, dtmi):
        expected_json = mock_http_get.return_value.json.return_value
        result = resolver.resolve(dtmi, endpoint)
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[dtmi] == expected_json

    @pytest.mark.it("Extracts a JSON dict from a JSON list before returning, if the HTTP GET returns a single JSON dict wrapped in a list")
    def test_removes_list_wrapper(self, mocker, mock_http_get, endpoint, dtmi):
        expected_json = mock_http_get.return_value.json.return_value
        # wrap json in a list
        mock_http_get.return_value.json.return_value = [mock_http_get.return_value.json.return_value]
        result = resolver.resolve(dtmi, endpoint)
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[dtmi] == expected_json

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize("dtmi", [
        pytest.param("", id="Empty string"),
        pytest.param("not a dtmi", id="Not a DTMI"),
        pytest.param("com:somedomain:example:FakeDTDL;1", id="DTMI missing scheme"),
        pytest.param("dtmi:com:somedomain:example:FakeDTDL", id="DTMI missing version"),
        pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI")
    ])
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint)

    @pytest.mark.it("Raises a ResolverError if the HTTP GET is unsuccessful (not a 200 response)")
    def test_get_failure(self, mock_http_get, endpoint, dtmi):
        mock_http_get.return_value.status_code = 400
        with pytest.raises(resolver.ResolverError):
            resolver.resolve(dtmi, endpoint)


@pytest.mark.describe(".resolve() -- Local filesystem endpoint")
class TestResolverFilesystem(SharedResolverTests):
    @pytest.fixture
    def endpoint(self):
        return "C:/repository/"

    @pytest.fixture
    def mock_open(self, mocker, fake_json):
        return mocker.patch('builtins.open', mocker.mock_open(read_data=json.dumps(fake_json)))

    @pytest.mark.it("Performs a file open/read on a filepath to a .json file created from combining the endpoint and DTMI")
    @pytest.mark.parametrize("endpoint, dtmi, expected_path", [
        pytest.param("C:/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "C:/repository/dtmi/com/somedomain/example/fakedtdl-1.json", id="Windows Filesystem"),
        pytest.param("C:/repository", "dtmi:com:somedomain:example:FakeDTDL;1", "C:/repository/dtmi/com/somedomain/example/fakedtdl-1.json", id="Windows Filesystem, no trailing '/'"),
        pytest.param("file://c:/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "c:/repository/dtmi/com/somedomain/example/fakedtdl-1.json", id="Windows Filesystem, File URI scheme"),
        pytest.param("/home/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "/home/repository/dtmi/com/somedomain/example/fakedtdl-1.json", id="POSIX Filesystem"),
        pytest.param("/home/repository", "dtmi:com:somedomain:example:FakeDTDL;1", "/home/repository/dtmi/com/somedomain/example/fakedtdl-1.json", id="POSIX Filesystem, no trailing '/'"),
        pytest.param("file:///home/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "/home/repository/dtmi/com/somedomain/example/fakedtdl-1.json", id="POSIX Filesystem, File URI scheme"),
    ])
    def test_regular_filepath(self, mocker, mock_open, endpoint, dtmi, expected_path):
        resolver.resolve(dtmi, endpoint)

        assert mock_open.call_count == 1
        assert mock_open.call_args == mocker.call(expected_path)
        assert mock_open.return_value.read.call_count == 1

    @pytest.mark.it("Performs an file open/read on a filepath to a .expanded.json file created from combining the endpoint and DTMI, if the 'expanded' arg is True")
    @pytest.mark.parametrize("endpoint, dtmi, expected_path", [
        pytest.param("C:/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "C:/repository/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="Windows Filesystem"),
        pytest.param("C:/repository", "dtmi:com:somedomain:example:FakeDTDL;1", "C:/repository/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="Windows Filesystem, no trailing '/'"),
        pytest.param("file://c:/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "c:/repository/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="Windows Filesystem, File URI scheme"),
        pytest.param("/home/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "/home/repository/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="POSIX Filesystem"),
        pytest.param("/home/repository", "dtmi:com:somedomain:example:FakeDTDL;1", "/home/repository/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="POSIX Filesystem, no trailing '/'"),
        pytest.param("file:///home/repository/", "dtmi:com:somedomain:example:FakeDTDL;1", "/home/repository/dtmi/com/somedomain/example/fakedtdl-1.expanded.json", id="POSIX Filesystem, File URI scheme"),
    ])
    def test_expanded_filepath(self, mocker, mock_open, endpoint, dtmi, expected_path):
        resolver.resolve(dtmi, endpoint, expanded=True)

        assert mock_open.call_count == 1
        assert mock_open.call_args == mocker.call(expected_path)
        assert mock_open.return_value.read.call_count == 1

    @pytest.mark.it("Returns a dictionary mapping the provided DTMI to the result of the file read")
    def test_returns_dict(self, mocker, mock_open, endpoint, dtmi, fake_json):
        # when you read after doing a mock_open, it will return fake_json in string form, not JSON form.
        # This will later get converted back into JSON form (i.e. value of fake_json fixture)
        # Instead of comparing the value directly to the mock as in other tests, we gotta directly compare
        # to the expected value of fake_json because mock_open uses side_effects
        result = resolver.resolve(dtmi, endpoint)
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[dtmi] == fake_json

    @pytest.mark.it("Extracts a JSON dict from a JSON list before returning, if the file read returns a single JSON dict wrapped in a list")
    def test_removes_list_wrapper(self, mocker, mock_open, endpoint, dtmi, fake_json):
        # make a new side effect for reading the file that gives a different result
        original_side_effect = mock_open.return_value.read.side_effect
        def wrapping_side_effect(*args, **kwargs):
            # the result will be the fake_json as it was set in the fixture
            res = original_side_effect(*args, **kwargs)
            return [res]
        mock_open.return_value.side_effect = wrapping_side_effect

        result = resolver.resolve(dtmi, endpoint)
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[dtmi] == fake_json

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize("dtmi", [
        pytest.param("", id="Empty string"),
        pytest.param("not a dtmi", id="Not a DTMI"),
        pytest.param("com:somedomain:example:FakeDTDL;1", id="DTMI missing scheme"),
        pytest.param("dtmi:com:somedomain:example:FakeDTDL", id="DTMI missing version"),
        pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI")
    ])
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint)