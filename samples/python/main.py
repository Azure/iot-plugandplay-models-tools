from azure.iot.modelsrepository import resolver
import pprint

repository_endpoint = "https://devicemodels.azure.com"
#dtmi = 'dtmi:azure:DeviceManagement:DeviceInformation;1'
dtmi = "dtmi:com:example:TemperatureController;1"

a = resolver.resolve(dtmi, repository_endpoint, resolve_dependencies=True)
pprint.pprint(a)