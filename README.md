# Constellation Package for Nest devices

This package connect your Nest devices into your Constellation.

The package push as StateObjects your Nest structure and Thermostat.

### News
   - Added Smoke CO alarm support
   - Added multi device targetting support

### Incoming
   - All Specific Smoke CO alarms implementated

### MessageCallbacks
  - SetAwayMode(bool, string id = "") : Sets the away mode to the first structure, or the specified one.
  - SetTargetTemperature(double, string id = "") : Sets the target temperature for the first thermostat, or the specified one.
  - SetProperty(string path, string propertyName, object value) : Sets the property for a specified path.

### Installation

Declare the package in a Sentinel with the following configuration :
```xml
<package name="Nest">
  <settings>
	<setting key="AccessToken" value="<access key>" />
	<setting key="ClientId" value="<client id>" />
	<setting key="ClientSecret" value="<client secret>" />            
  </settings>
</package>
```
* To create your Nest Client ID & Secret : https://developer.nest.com/
* To get an Access Key, call the static method : AuthentificationManager.GetAccessToken()

License
----

Apache License
