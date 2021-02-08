# Azure IoT Models Repository Library Samples

This directory contains samples showing how to use the features of the Azure IoT Models Repository Library.

The pre-configured endpoints and DTMIs within the sampmles refer to example DTDL documents that can be found on [devicemodels.azure.com](https://devicemodels.azure.com/). These values can be replaced to reflect the locations of your own DTDLs, wherever they may be.

## Resolver Samples
* [resolve_sample.py](resolve_sample.py) - Retrieve the contents of a DTDL .json document based on a given endpoint and DTMI

* [resolve_from_expanded_sample.py](resolve_from_expanded_sample.py) - Retrieve the contents of a DTDL .expanded.json document based on a given endpoint and DTMI

* [resolve_dependencies_sample.py](resolve_dependencies_sample.py) - Retreive the contents of a DTDL .json document, as well as the contents of all specified dependency documents within it, given an endpoint and DTMI