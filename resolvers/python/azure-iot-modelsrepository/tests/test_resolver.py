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
def dtdl_json():
    return {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:FakeDTDL;1",
        "@type": "Interface",
        "dispalyName": "someval",
        "contents": []
    }

@pytest.fixture
def dtdl_expanded_json():
    return [
        {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:FakeDTDL;1",
        "@type": "Interface",
        "dispalyName": "FakeDTDL1",
        "contents": []
        },
        {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:FakeDTDL;2",
        "@type": "Interface",
        "dispalyName": "FakeDTDL2",
        "contents": []
        },
        {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:FakeDTDL;3",
        "@type": "Interface",
        "dispalyName": "FakeDTDL3",
        "contents": []
    }
    ]


@pytest.mark.describe(".resolve() -- Remote URL endpoint")
class TestResolverURL(SharedResolverTests):
    @pytest.fixture
    def endpoint(self):
        return "https://somedomain.com/"

    @pytest.fixture
    def mock_http_get(self, mocker, dtdl_json, dtdl_expanded_json):
        mock_http_get = mocker.patch.object(requests, "get")
        mock_response = mock_http_get.return_value
        mock_response.status_code = 200

        def choose_json():
            """Choose the correct JSON to return based on what the get was called with"""
            url = mock_http_get.call_args[0][0]
            if url.endswith(".expanded.json"):
                return dtdl_expanded_json
            else:
                return dtdl_json

        mock_response.json.side_effect = choose_json
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

    @pytest.mark.it("Returns a dictionary mapping the provided DTMI to its corresponding non-expanded DTDL returned by the HTTP GET, if the GET is successful (200 response)")
    def test_returned_dict_non_expanded(self, mocker, mock_http_get, endpoint, dtmi):
        result = resolver.resolve(dtmi, endpoint)
        expected_json = mock_http_get.return_value.json()
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[dtmi] == expected_json

    @pytest.mark.it("Returns a dictionary mapping DTMIs to corresponding DTDLs, for all components of an expanded DTDL returned by the HTTP GET, if the GET is successful (200 response)")
    def test_returned_dict_expanded(self, mocker, mock_http_get, endpoint, dtmi):
        result = resolver.resolve(dtmi, endpoint, expanded=True)
        received_json = mock_http_get.return_value.json()
        assert isinstance(result, dict)
        assert len(result) == len(received_json)
        for dtdl in received_json:
            dtmi = dtdl["@id"]
            assert dtmi in result.keys()
            assert result[dtmi] == dtdl

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
    def mock_open(self, mocker, dtdl_json, dtdl_expanded_json):
        mock_open = mocker.patch('builtins.open', mocker.mock_open())

        def choose_json():
            fpath = mock_open.call_args[0][0]
            if fpath.endswith(".expanded.json"):
                return json.dumps(dtdl_expanded_json)
            else:
                return json.dumps(dtdl_json)

        fh_mock = mock_open.return_value
        fh_mock.read.side_effect = choose_json
        return mock_open

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

    @pytest.mark.it("Returns a dictionary mapping the provided DTMI to its corresponding non-expanded DTDL returned as a result of the file read")
    def test_returns_dict_non_expanded(self, mocker, mock_open, endpoint, dtmi):
        result = resolver.resolve(dtmi, endpoint)
        expected_json = json.loads(mock_open.return_value.read())
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[dtmi] == expected_json

    @pytest.mark.it("Returns a dictionary mapping DTMIs to corresponding DTDLs, for all components of an expanded DTDL returned as a result of the file read")
    def test_returns_dict_expanded(self, mocker, mock_open, endpoint, dtmi):
        result = resolver.resolve(dtmi, endpoint, expanded=True)
        received_json = json.loads(mock_open.return_value.read())
        assert isinstance(result, dict)
        assert len(result) == len(received_json)
        for dtdl in received_json:
            dtmi = dtdl["@id"]
            assert dtmi in result.keys()
            assert result[dtmi] == dtdl

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
