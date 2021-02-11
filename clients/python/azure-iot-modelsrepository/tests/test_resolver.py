# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------
import pytest
import requests
import json
from azure.iot.modelsrepository import resolver


@pytest.fixture
def arbitrary_exception():
    class ArbitraryException(Exception):
        pass

    return ArbitraryException("This exception is completely arbitrary")


@pytest.fixture
def foo_dtmi():
    return "dtmi:com:somedomain:example:FooDTDL;1"


@pytest.fixture
def foo_dtdl_json():
    # Testing Notes:
    #   - Contains a single property
    #   - Contains multiple components
    #   - Contains an extension of an interface
    #   - Note that the 'Bar' component itself will also contain 'Buzz' as a component
    return {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:FooDTDL;1",
        "@type": "Interface",
        "displayName": "Foo",
        "extends": "dtmi:com:somedomain:example:BazDTDL;1",
        "contents": [
            {
                "@type": "Property",
                "name": "fooproperty",
                "displayName": "Foo Property",
                "schema": "string",
                "description": "A string representing some value. This isn't real",
            },
            {
                "@type": "Component",
                "name": "bar",
                "displayName": "Bar",
                "schema": "dtmi:com:somedomain:example:BarDTDL;1",
                "description": "Bar component",
            },
            {
                "@type": "Component",
                "name": "buzz",
                "displayName": "Buzz",
                "schema": "dtmi:com:somedomain:example:BuzzDTDL;1",
            },
        ],
    }


@pytest.fixture
def bar_dtdl_json():
    # Testing Notes:
    #   - Contains a component (while itself being a component in another model)
    #   - Contains a telemetry
    return {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:BarDTDL;1",
        "@type": "Interface",
        "displayName": "Bar",
        "contents": [
            {
                "@type": "Property",
                "name": "barproperty",
                "displayName": "Bar Property",
                "schema": "string",
                "description": "A string representing some value. This isn't real",
            },
            {
                "@type": "Component",
                "name": "buzz",
                "displayName": "Buzz",
                "schema": "dtmi:com:somedomain:example:BuzzDTDL;1",
            },
            {
                "@type": "Telemetry",
                "name": "bartelemetry",
                "schema": "double"
            },
        ],
    }


@pytest.fixture
def buzz_dtdl_json():
    # Testing Notes:
    #   - Contains two extensions of interfaces (maximum value)
    #   - Contains a single property
    return {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:BuzzDTDL;1",
        "@type": "Interface",
        "displayName": "Buzz",
        "extends": ["dtmi:com:somedomain:example:QuxDTDL;1", "dtmi:com:somedomain:example:QuzDTDL;1"],
        "contents": [
            {
                "@type": "Property",
                "name": "buzzproperty",
                "displayName": "Buzz Property",
                "schema": "string",
                "description": "A string representing some value. This isn't real",
            },
        ],
    }

@pytest.fixture
def baz_dtdl_json():
    # Testing Notes:
    #   - Contains multiple properties
    return {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:BazDTDL;1",
        "@type": "Interface",
        "displayName": "Baz",
        "contents": [
            {
                "@type": "Property",
                "name": "bazproperty1",
                "displayName": "Baz Property 1",
                "schema": "string",
                "description": "A string representing some value. This isn't real",
            },
            {
                "@type": "Property",
                "name": "bazproperty2",
                "displayName": "Baz Property 2",
                "schema": "string",
                "description": "A string representing some value. This isn't real",
            },
        ]
    }

@pytest.fixture
def qux_dtdl_json():
    # Testing Notes:
    #   - Contains a Command
    return {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:QuxDTDL;1",
        "@type": "Interface",
        "displayName": "Qux",
        "contents": [
            {
                "@type": "Command",
                "name": "quxcommand",
                "request": {
                    "name": "quxcommandtime",
                    "displayName": "Qux Command Time",
                    "description": "It's a command. For Qux.",
                    "schema": "dateTime"
                },
                "response": {
                    "name": "quxresponsetime",
                    "schema": "dateTime"
                }
            }

        ]
    }

@pytest.fixture
def quz_dtdl_json():
    # Testing Notes:
    #   - Contains no contents (doesn't make much sense, but an edge case to test nontheless)
    return {
        "@context": "dtmi:dtdl:context;1",
        "@id": "dtmi:com:somedomain:example:QuzDTDL;1",
        "@type": "Interface",
        "displayName": "Quz",
    }



@pytest.fixture
def foo_dtdl_expanded_json(foo_dtdl_json, bar_dtdl_json, buzz_dtdl_json, qux_dtdl_json, quz_dtdl_json, baz_dtdl_json):
    return [foo_dtdl_json, bar_dtdl_json, buzz_dtdl_json, qux_dtdl_json, quz_dtdl_json, baz_dtdl_json]


class ResolveFromRemoteURLEndpointTestConfig(object):
    @pytest.fixture
    def endpoint(self):
        return "https://somedomain.com/"

    @pytest.fixture
    def mock_http_get(
        self, mocker, foo_dtdl_json, bar_dtdl_json, buzz_dtdl_json, baz_dtdl_json, qux_dtdl_json, quz_dtdl_json, foo_dtdl_expanded_json
    ):
        mock_http_get = mocker.patch.object(requests, "get")
        mock_response = mock_http_get.return_value
        mock_response.status_code = 200
        mock_http_get.cached_json_responses = (
            []
        )  # cache returned json for tests involving multiple calls

        def choose_json():
            """Choose the correct JSON to return based on what the get was called with"""
            url = mock_http_get.call_args[0][0]
            if "FooDTDL".lower() in url and url.endswith(".expanded.json"):
                return foo_dtdl_expanded_json
            else:
                if "FooDTDL".lower() in url:
                    mock_http_get.cached_json_responses.append(foo_dtdl_json)
                    return foo_dtdl_json
                elif "BarDTDL".lower() in url:
                    mock_http_get.cached_json_responses.append(bar_dtdl_json)
                    return bar_dtdl_json
                elif "BuzzDTDL".lower() in url:
                    mock_http_get.cached_json_responses.append(buzz_dtdl_json)
                    return buzz_dtdl_json
                elif "BazDTDL".lower() in url:
                    mock_http_get.cached_json_responses.append(baz_dtdl_json)
                    return baz_dtdl_json
                elif "QuxDTDL".lower() in url:
                    mock_http_get.cached_json_responses.append(qux_dtdl_json)
                    return qux_dtdl_json
                elif "QuzDTDL".lower() in url:
                    mock_http_get.cached_json_responses.append(quz_dtdl_json)
                    return quz_dtdl_json
                else:
                    return "no corresponding json :("

        mock_response.json.side_effect = choose_json
        return mock_http_get


@pytest.mark.describe(".resolve() -- Remote URL endpoint")
class TestResolveFromRemoteURLEndpoint(ResolveFromRemoteURLEndpointTestConfig):
    @pytest.mark.it(
        "Performs an HTTP GET on a URL path to a .json file specified by the combination of the provided endpoint and DTMI"
    )
    @pytest.mark.parametrize(
        "endpoint, dtmi, expected_url",
        [
            pytest.param(
                "http://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "http://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                id="HTTP endpoint",
            ),
            pytest.param(
                "https://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                id="HTTPS endpoint",
            ),
            pytest.param(
                "ftp://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "ftp://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                id="FTP endpoint",
            ),
            pytest.param(
                "http://somedomain.com",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "http://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                id="Endpoint with no trailing '/'",
            ),
            pytest.param(
                "somedomain.com",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                id="Endpoint with no specified protocol",
            ),
        ],
    )
    def test_http_get(self, mocker, mock_http_get, endpoint, dtmi, expected_url):
        resolver.resolve(dtmi, endpoint)

        assert mock_http_get.call_count == 1
        assert mock_http_get.call_args == mocker.call(expected_url)

    @pytest.mark.it(
        "Returns a dictionary mapping the provided DTMI to its corresponding DTDL returned by the HTTP GET, if the GET is successful (200 response)"
    )
    def test_returned_dict(self, mocker, mock_http_get, endpoint, foo_dtmi):
        result = resolver.resolve(foo_dtmi, endpoint)
        expected_json = mock_http_get.return_value.json()
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[foo_dtmi] == expected_json

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize(
        "dtmi",
        [
            pytest.param("", id="Empty string"),
            pytest.param("not a dtmi", id="Not a DTMI"),
            pytest.param("com:somedomain:example:FooDTDL;1", id="DTMI missing scheme"),
            pytest.param("dtmi:com:somedomain:example:FooDTDL", id="DTMI missing version"),
            pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI"),
        ],
    )
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint)

    @pytest.mark.it("Raises a ValueError if the user-provided URL path is invalid")
    @pytest.mark.parametrize(
        "endpoint",
        [
            pytest.param("not an endpoint", id="Not an endpoint"),
            pytest.param("wasd://somedomain.com/", id="Unrecognized protocol"),
            pytest.param("someendpoint", id="Incomplete endpoint"),
        ],
    )
    def test_invalid_endpoint(self, foo_dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(foo_dtmi, endpoint)

    @pytest.mark.it("Raises a ResolverError if the HTTP GET is unsuccessful (not a 200 response)")
    def test_get_failure(self, mock_http_get, endpoint, foo_dtmi):
        mock_http_get.return_value.status_code = 400
        with pytest.raises(resolver.ResolverError):
            resolver.resolve(foo_dtmi, endpoint)


@pytest.mark.describe(".resolve() -- Remote URL endpoint (Expanded DTDL)")
class TestResolveFromRemoteURLEndpointWithExpanded(ResolveFromRemoteURLEndpointTestConfig):
    @pytest.mark.it(
        "Performs an HTTP GET on a URL path to a .expanded.json file specified by the combination of the provided endpoint and DTMI"
    )
    @pytest.mark.parametrize(
        "endpoint, dtmi, expected_url",
        [
            pytest.param(
                "http://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "http://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="HTTP endpoint",
            ),
            pytest.param(
                "https://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="HTTPS endpoint",
            ),
            pytest.param(
                "ftp://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "ftp://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="FTP endpoint",
            ),
            pytest.param(
                "http://somedomain.com",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "http://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="Endpoint with no trailing '/'",
            ),
            pytest.param(
                "somedomain.com",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="Endpoint with no specified protocol",
            ),
        ],
    )
    def test_http_get(self, mocker, mock_http_get, endpoint, dtmi, expected_url):
        resolver.resolve(dtmi, endpoint, expanded=True)

        assert mock_http_get.call_count == 1
        assert mock_http_get.call_args == mocker.call(expected_url)

    @pytest.mark.it(
        "Returns a dictionary mapping DTMIs to corresponding DTDLs, for all elements of the expanded.json file returned by the HTTP GET, if the GET is successful (200 response)"
    )
    def test_returned_dict(self, mocker, mock_http_get, endpoint, foo_dtmi):
        result = resolver.resolve(foo_dtmi, endpoint, expanded=True)
        received_json = mock_http_get.return_value.json()
        assert isinstance(result, dict)
        assert len(result) == len(received_json)
        for dtdl in received_json:
            dtmi = dtdl["@id"]
            assert dtmi in result.keys()
            assert result[dtmi] == dtdl

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize(
        "dtmi",
        [
            pytest.param("", id="Empty string"),
            pytest.param("not a dtmi", id="Not a DTMI"),
            pytest.param("com:somedomain:example:FooDTDL;1", id="DTMI missing scheme"),
            pytest.param("dtmi:com:somedomain:example:FooDTDL", id="DTMI missing version"),
            pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI"),
        ],
    )
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint, expanded=True)

    @pytest.mark.it("Raises a ValueError if the user-provided URL path is invalid")
    @pytest.mark.parametrize(
        "endpoint",
        [
            pytest.param("not an endpoint", id="Not an endpoint"),
            pytest.param("wasd://somedomain.com/", id="Unrecognized protocol"),
            pytest.param("someendpoint", id="Incomplete endpoint"),
        ],
    )
    def test_invalid_endpoint(self, foo_dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(foo_dtmi, endpoint, expanded=True)

    @pytest.mark.it("Raises a ResolverError if the HTTP GET is unsuccessful (not a 200 response)")
    def test_get_failure(self, mock_http_get, endpoint, foo_dtmi):
        mock_http_get.return_value.status_code = 400
        with pytest.raises(resolver.ResolverError):
            resolver.resolve(foo_dtmi, endpoint, expanded=True)


@pytest.mark.describe(".resolve() -- Remote URL endpoint (Resolve DTDL Dependencies)")
class TestResolveFromRemoteURLEndpointWithDependencyResolution(
    ResolveFromRemoteURLEndpointTestConfig
):
    @pytest.mark.it(
        "Performs an HTTP GET on the URL path to .json file specified by the combination of the provided endpoint and DTMI, as well as on the URL paths for all unique component and extended interface DTMIs"
    )
    @pytest.mark.parametrize(
        "endpoint, dtmi, expected_url1, expected_url2, expected_url3, expected_url4, expected_url5, expected_url6",
        [
            pytest.param(
                "http://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "http://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/bardtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/quxdtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/quzdtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="HTTP endpoint",
            ),
            pytest.param(
                "https://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/bardtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/quxdtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/quzdtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="HTTPS endpoint",
            ),
            pytest.param(
                "ftp://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "ftp://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                "ftp://somedomain.com/dtmi/com/somedomain/example/bardtdl-1.json",
                "ftp://somedomain.com/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "ftp://somedomain.com/dtmi/com/somedomain/example/quxdtdl-1.json",
                "ftp://somedomain.com/dtmi/com/somedomain/example/quzdtdl-1.json",
                "ftp://somedomain.com/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="FTP endpoint",
            ),
            pytest.param(
                "http://somedomain.com",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "http://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/bardtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/quxdtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/quzdtdl-1.json",
                "http://somedomain.com/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="Endpoint with no trailing '/'",
            ),
            pytest.param(
                "somedomain.com",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/bardtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/quxdtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/quzdtdl-1.json",
                "https://somedomain.com/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="Endpoint with no specified protocol",
            ),
        ],
    )
    # NOTE: these multiple URL parameters come from the definition of the DTDL fixtures.
    def test_http_get(
        self, mocker, mock_http_get, endpoint, dtmi, expected_url1, expected_url2, expected_url3, expected_url4, expected_url5, expected_url6
    ):
        resolver.resolve(dtmi, endpoint, resolve_dependencies=True)

        # NOTE: there are 6 calls, because we only do a GET for each UNIQUE component.
        # The BuzzDTDL is included twice in the structure, but only needs one GET call.
        assert mock_http_get.call_count == 6
        assert mock_http_get.call_args_list[0] == mocker.call(expected_url1)
        assert mock_http_get.call_args_list[1] == mocker.call(expected_url2)
        assert mock_http_get.call_args_list[2] == mocker.call(expected_url3)
        assert mock_http_get.call_args_list[3] == mocker.call(expected_url4)
        assert mock_http_get.call_args_list[4] == mocker.call(expected_url5)
        assert mock_http_get.call_args_list[5] == mocker.call(expected_url6)

    @pytest.mark.it(
        "Returns a dictionary mapping DTMIs to corresponding DTDLs from .json files returned by the HTTP GET operations, if the GETs are successful (200 response)"
    )
    def test_returned_dict(self, mocker, mock_http_get, endpoint, foo_dtmi):
        result = resolver.resolve(foo_dtmi, endpoint, resolve_dependencies=True)
        assert isinstance(result, dict)
        assert (
            len(mock_http_get.cached_json_responses) == len(result) == mock_http_get.call_count == 6
        )
        for dtdl in mock_http_get.cached_json_response:
            dtmi = dtdl["@id"]
            assert dtmi in result.keys()
            assert result[dtmi] == dtdl

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize(
        "dtmi",
        [
            pytest.param("", id="Empty string"),
            pytest.param("not a dtmi", id="Not a DTMI"),
            pytest.param("com:somedomain:example:FooDTDL;1", id="DTMI missing scheme"),
            pytest.param("dtmi:com:somedomain:example:FooDTDL", id="DTMI missing version"),
            pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI"),
        ],
    )
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint, resolve_dependencies=True)

    @pytest.mark.it("Raises a ValueError if the user-provided URL path is invalid")
    @pytest.mark.parametrize(
        "endpoint",
        [
            pytest.param("not an endpoint", id="Not an endpoint"),
            pytest.param("wasd://somedomain.com/", id="Unrecognized protocol"),
            pytest.param("someendpoint", id="Incomplete endpoint"),
        ],
    )
    def test_invalid_endpoint(self, foo_dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(foo_dtmi, endpoint, resolve_dependencies=True)

    @pytest.mark.it("Raises a ResolverError if the HTTP GET is unsuccessful (not a 200 response)")
    def test_get_failure(self, mock_http_get, endpoint, foo_dtmi):
        mock_http_get.return_value.status_code = 400
        with pytest.raises(resolver.ResolverError):
            resolver.resolve(foo_dtmi, endpoint, resolve_dependencies=True)


class ResolveFromLocalFilesystemEndpointTestConfig(object):
    @pytest.fixture
    def endpoint(self):
        return "C:/repository/"

    @pytest.fixture
    def mock_open(
        self, mocker, foo_dtdl_json, bar_dtdl_json, buzz_dtdl_json, qux_dtdl_json, quz_dtdl_json, baz_dtdl_json, foo_dtdl_expanded_json
    ):
        mock_open = mocker.patch("builtins.open", mocker.mock_open())
        fh_mock = mock_open.return_value
        fh_mock.read.cached_json_responses = []

        def choose_json():
            fpath = mock_open.call_args[0][0]
            if "FooDTDL".lower() in fpath and fpath.endswith(".expanded.json"):
                return json.dumps(foo_dtdl_expanded_json)
            else:
                if "FooDTDL".lower() in fpath:
                    fh_mock.read.cached_json_responses.append(foo_dtdl_json)
                    return json.dumps(foo_dtdl_json)
                elif "BarDTDL".lower() in fpath:
                    fh_mock.read.cached_json_responses.append(bar_dtdl_json)
                    return json.dumps(bar_dtdl_json)
                elif "BuzzDTDL".lower() in fpath:
                    fh_mock.read.cached_json_responses.append(buzz_dtdl_json)
                    return json.dumps(buzz_dtdl_json)
                elif "BazDTDL".lower() in fpath:
                    fh_mock.read.cached_json_responses.append(baz_dtdl_json)
                    return json.dumps(baz_dtdl_json)
                elif "QuxDTDL".lower() in fpath:
                    fh_mock.read.cached_json_responses.append(qux_dtdl_json)
                    return json.dumps(qux_dtdl_json)
                elif "QuzDTDL".lower() in fpath:
                    fh_mock.read.cached_json_responses.append(quz_dtdl_json)
                    return json.dumps(quz_dtdl_json)
                else:
                    return "no corresponding json :("

        fh_mock.read.side_effect = choose_json
        return mock_open


@pytest.mark.describe(".resolve() -- Local filesystem endpoint")
class TestResolveFromLocalFilesystemEndpoint(ResolveFromLocalFilesystemEndpointTestConfig):
    @pytest.mark.it(
        "Performs a file open/read on a filepath to a .json file specified by the combinination of the provided endpoint and DTMI"
    )
    @pytest.mark.parametrize(
        "endpoint, dtmi, expected_path",
        [
            pytest.param(
                "C:/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "C:/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                id="Windows Filesystem",
            ),
            pytest.param(
                "C:/repository",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "C:/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                id="Windows Filesystem, no trailing '/'",
            ),
            pytest.param(
                "file://c:/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "c:/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                id="Windows Filesystem, File URI scheme",
            ),
            pytest.param(
                "/home/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                id="POSIX Filesystem",
            ),
            pytest.param(
                "/home/repository",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                id="POSIX Filesystem, no trailing '/'",
            ),
            pytest.param(
                "file:///home/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                id="POSIX Filesystem, File URI scheme",
            ),
        ],
    )
    def test_open(self, mocker, mock_open, endpoint, dtmi, expected_path):
        resolver.resolve(dtmi, endpoint)

        assert mock_open.call_count == 1
        assert mock_open.call_args == mocker.call(expected_path)
        assert mock_open.return_value.read.call_count == 1

    @pytest.mark.it(
        "Returns a dictionary mapping the provided DTMI to its corresponding DTDL returned as a result of the file read"
    )
    def test_returned_dict(self, mocker, mock_open, endpoint, foo_dtmi):
        result = resolver.resolve(foo_dtmi, endpoint)
        expected_json = json.loads(mock_open.return_value.read())
        assert isinstance(result, dict)
        assert len(result) == 1
        assert result[foo_dtmi] == expected_json

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize(
        "dtmi",
        [
            pytest.param("", id="Empty string"),
            pytest.param("not a dtmi", id="Not a DTMI"),
            pytest.param("com:somedomain:example:FooDTDL;1", id="DTMI missing scheme"),
            pytest.param("dtmi:com:somedomain:example:FooDTDL", id="DTMI missing version"),
            pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI"),
        ],
    )
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint)

    @pytest.mark.it("Raises a ValueError if the user-provided URL path is invalid")
    @pytest.mark.parametrize(
        "endpoint",
        [
            pytest.param("not an endpoint", id="Not an endpoint"),
            pytest.param("wasd://somedomain.com/", id="Unrecognized protocol"),
            pytest.param("someendpoint", id="Incomplete endpoint"),
        ],
    )
    def test_invalid_endpoint(self, foo_dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(foo_dtmi, endpoint)

    @pytest.mark.it("Raises a ResolverError if the file open/read is unsuccessful")
    def test_read_open_failure(self, mock_open, endpoint, foo_dtmi, arbitrary_exception):
        mock_open.side_effect = arbitrary_exception

        with pytest.raises(resolver.ResolverError) as e_info:
            resolver.resolve(foo_dtmi, endpoint)
        assert e_info.value.__cause__ == arbitrary_exception


@pytest.mark.describe(".resolve() -- Local filesystem endpoint (Expanded DTDL)")
class TestResolveFromLocalFilesystemEndpointWithExpanded(
    ResolveFromLocalFilesystemEndpointTestConfig
):
    @pytest.mark.it(
        "Performs a file open/read on a filepath to a .expanded.json file specified by the combination of the provided endpoint and DTMI"
    )
    @pytest.mark.parametrize(
        "endpoint, dtmi, expected_path",
        [
            pytest.param(
                "C:/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "C:/repository/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="Windows Filesystem",
            ),
            pytest.param(
                "C:/repository",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "C:/repository/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="Windows Filesystem, no trailing '/'",
            ),
            pytest.param(
                "file://c:/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "c:/repository/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="Windows Filesystem, File URI scheme",
            ),
            pytest.param(
                "/home/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="POSIX Filesystem",
            ),
            pytest.param(
                "/home/repository",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="POSIX Filesystem, no trailing '/'",
            ),
            pytest.param(
                "file:///home/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.expanded.json",
                id="POSIX Filesystem, File URI scheme",
            ),
        ],
    )
    def test_open(self, mocker, mock_open, endpoint, dtmi, expected_path):
        resolver.resolve(dtmi, endpoint, expanded=True)

        assert mock_open.call_count == 1
        assert mock_open.call_args == mocker.call(expected_path)
        assert mock_open.return_value.read.call_count == 1

    @pytest.mark.it(
        "Returns a dictionary mapping DTMIs to corresponding DTDLs, for all components of an expanded DTDL returned as a result of the file read"
    )
    def test_returned_dict(self, mocker, mock_open, endpoint, foo_dtmi):
        result = resolver.resolve(foo_dtmi, endpoint, expanded=True)
        received_json = json.loads(mock_open.return_value.read())
        assert isinstance(result, dict)
        assert len(result) == len(received_json)
        for dtdl in received_json:
            dtmi = dtdl["@id"]
            assert dtmi in result.keys()
            assert result[dtmi] == dtdl

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize(
        "dtmi",
        [
            pytest.param("", id="Empty string"),
            pytest.param("not a dtmi", id="Not a DTMI"),
            pytest.param("com:somedomain:example:FooDTDL;1", id="DTMI missing scheme"),
            pytest.param("dtmi:com:somedomain:example:FooDTDL", id="DTMI missing version"),
            pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI"),
        ],
    )
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint, expanded=True)

    @pytest.mark.it("Raises a ValueError if the user-provided URL path is invalid")
    @pytest.mark.parametrize(
        "endpoint",
        [
            pytest.param("not an endpoint", id="Not an endpoint"),
            pytest.param("wasd://somedomain.com/", id="Unrecognized protocol"),
            pytest.param("someendpoint", id="Incomplete endpoint"),
        ],
    )
    def test_invalid_endpoint(self, foo_dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(foo_dtmi, endpoint, expanded=True)

    @pytest.mark.it("Raises a ResolverError if the file open/read is unsuccessful")
    def test_read_open_failure(self, mock_open, endpoint, foo_dtmi, arbitrary_exception):
        mock_open.side_effect = arbitrary_exception

        with pytest.raises(resolver.ResolverError) as e_info:
            resolver.resolve(foo_dtmi, endpoint, expanded=True)
        assert e_info.value.__cause__ == arbitrary_exception


@pytest.mark.describe(".resolve() -- Local filesystem endpoint (Resolve DTDL dependencies)")
class TestResolveFromLocalFilesystemEndpointWithDependencyResolution(
    ResolveFromLocalFilesystemEndpointTestConfig
):
    @pytest.mark.it(
        "Performs a file open/read on a filepath to a .json file, specified by the combination of the endpoint and DTMI, as well as on the filepaths for all unique component and extended interface DTMIs"
    )
    @pytest.mark.parametrize(
        "endpoint, dtmi, expected_path1, expected_path2, expected_path3, expected_path4, expected_path5, expected_path6",
        [
            pytest.param(
                "C:/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "C:/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/bardtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/quxdtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/quzdtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="Windows Filesystem",
            ),
            pytest.param(
                "C:/repository",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "C:/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/bardtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/quxdtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/quzdtdl-1.json",
                "C:/repository/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="Windows Filesystem, no trailing '/'",
            ),
            pytest.param(
                "file://c:/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "c:/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                "c:/repository/dtmi/com/somedomain/example/bardtdl-1.json",
                "c:/repository/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "c:/repository/dtmi/com/somedomain/example/quxdtdl-1.json",
                "c:/repository/dtmi/com/somedomain/example/quzdtdl-1.json",
                "c:/repository/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="Windows Filesystem, File URI scheme",
            ),
            pytest.param(
                "/home/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/bardtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/quxdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/quzdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="POSIX Filesystem",
            ),
            pytest.param(
                "/home/repository",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/bardtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/quxdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/quzdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="POSIX Filesystem, no trailing '/'",
            ),
            pytest.param(
                "file:///home/repository/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "/home/repository/dtmi/com/somedomain/example/foodtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/bardtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/buzzdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/quxdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/quzdtdl-1.json",
                "/home/repository/dtmi/com/somedomain/example/bazdtdl-1.json",
                id="POSIX Filesystem, File URI scheme",
            ),
        ],
    )
    def test_open(
        self, mocker, mock_open, endpoint, dtmi, expected_path1, expected_path2, expected_path3, expected_path4, expected_path5, expected_path6
    ):
        resolver.resolve(dtmi, endpoint, resolve_dependencies=True)

        # NOTE: there are 6 calls, because we only do an open/read for each UNIQUE component.
        # The BuzzDTDL is included twice in the structure, but is only opened/read once.
        assert mock_open.call_count == 6
        assert mock_open.return_value.read.call_count == 6
        assert mock_open.call_args_list[0] == mocker.call(expected_path1)
        assert mock_open.call_args_list[1] == mocker.call(expected_path2)
        assert mock_open.call_args_list[2] == mocker.call(expected_path3)
        assert mock_open.call_args_list[3] == mocker.call(expected_path4)
        assert mock_open.call_args_list[4] == mocker.call(expected_path5)
        assert mock_open.call_args_list[5] == mocker.call(expected_path6)

    @pytest.mark.it(
        "Returns a dictionary mapping DTMIs to corresponding DTDLs from .json files returned by the open/read operations"
    )
    def test_returned_dict(self, mocker, mock_open, endpoint, foo_dtmi):
        result = resolver.resolve(foo_dtmi, endpoint, resolve_dependencies=True)
        assert isinstance(result, dict)
        assert (
            len(mock_open.return_value.read.cached_json_responses)
            == len(result)
            == mock_open.call_count
            == mock_open.return_value.read.call_count
            == 6
        )
        for dtdl in mock_open.return_value.read.cached_json_responses:
            dtmi = dtdl["@id"]
            assert dtmi in result.keys()
            assert result[dtmi] == dtdl

    @pytest.mark.it("Raises a ValueError if the user-provided DTMI is invalid")
    @pytest.mark.parametrize(
        "dtmi",
        [
            pytest.param("", id="Empty string"),
            pytest.param("not a dtmi", id="Not a DTMI"),
            pytest.param("com:somedomain:example:FooDTDL;1", id="DTMI missing scheme"),
            pytest.param("dtmi:com:somedomain:example:FooDTDL", id="DTMI missing version"),
            pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI"),
        ],
    )
    def test_invalid_dtmi(self, dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(dtmi, endpoint, resolve_dependencies=True)

    @pytest.mark.it("Raises a ValueError if the user-provided URL path is invalid")
    @pytest.mark.parametrize(
        "endpoint",
        [
            pytest.param("not an endpoint", id="Not an endpoint"),
            pytest.param("wasd://somedomain.com/", id="Unrecognized protocol"),
            pytest.param("someendpoint", id="Incomplete endpoint"),
        ],
    )
    def test_invalid_endpoint(self, foo_dtmi, endpoint):
        with pytest.raises(ValueError):
            resolver.resolve(foo_dtmi, endpoint, resolve_dependencies=True)

    @pytest.mark.it("Raises a ResolverError if the file open/read is unsuccessful")
    def test_read_open_failure(self, mock_open, endpoint, foo_dtmi, arbitrary_exception):
        mock_open.side_effect = arbitrary_exception

        with pytest.raises(resolver.ResolverError) as e_info:
            resolver.resolve(foo_dtmi, endpoint, resolve_dependencies=True)
        assert e_info.value.__cause__ == arbitrary_exception


@pytest.mark.describe(".get_fully_qualified_dtmi()")
class TestGetFullyQualifiedDTMI(object):
    @pytest.mark.it(
        "Returns a fully qualified DTMI path for a .json file by combining the provided endpoint and DTMI"
    )
    @pytest.mark.parametrize(
        "endpoint, dtmi, expected_path",
        [
            pytest.param(
                "https://somedomain.com/",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                id="URL w/ trailing '/'",
            ),
            pytest.param(
                "https://somedomain.com",
                "dtmi:com:somedomain:example:FooDTDL;1",
                "https://somedomain.com/dtmi/com/somedomain/example/foodtdl-1.json",
                id="URL w/o trailing '/'",
            ),
        ],
    )
    def test_valid_path(self, endpoint, dtmi, expected_path):
        result = resolver.get_fully_qualified_dtmi(dtmi, endpoint)
        assert result == expected_path

    @pytest.mark.it("Raises a ValueError if the provided DTMI is invalid")
    @pytest.mark.parametrize(
        "dtmi",
        [
            pytest.param("", id="Empty string"),
            pytest.param("not a dtmi", id="Not a DTMI"),
            pytest.param("com:somedomain:example:FooDTDL;1", id="DTMI missing scheme"),
            pytest.param("dtmi:com:somedomain:example:FooDTDL", id="DTMI missing version"),
            pytest.param("dtmi:foo_bar:_16:baz33:qux;12", id="System DTMI"),
        ],
    )
    def test_invalid_dtmi(self, dtmi):
        with pytest.raises(ValueError):
            resolver.get_fully_qualified_dtmi(dtmi, "https://somedomain.com/")
