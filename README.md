# Trade Imports Data API Stub

This service provides stubbed responses for specific requests.

## Prerequisites

The solution requires:

- .NET 9

  ```bash
  brew tap isen-ng/dotnet-sdk-versions
  brew install --cask dotnet-sdk9
  ```

- Docker

## Installation

1. Clone this repository
2. Install the required tools with `dotnet tool restore`
3. Check the solution builds with `dotnet build`
4. Check the service builds with `docker build .`

## Running

1. Run the application via Docker:
   ```
   docker compose up
   ```
2. Navigate to http://localhost:8085 and see all endpoints supported

## Responses

See the Scenarios folder for all available responses. 

All endpoints currently active can be seen via http://localhost:8085/.

## Generating endpoints

See the ScenarioGenerator in the tools folder and follow the [README](tools/ScenarioGenerator/tests/README.md) for how scenarios should be generated.

## Utility endpoints

A utility endpoint has been provided that will generate the import pre notification updates content based on the import notifications available in the Scenarios folder. Please use this if you want to update the stub response for import pre notification updates.
```http request
http://localhost:8085/utility/import-pre-notification-updates
```

Copy the output and override [_import-pre-notification-updates.json](src/Stub/Scenarios/_import-pre-notification-updates.json).

## Development

Any new scenarios should be added to the Scenarios folder in the following format:

- An underscore denotes a `/`
- The .json file type will be removed at runtime if included

Regeneration of scenarios TBC.

## Testing

Run the service via Docker and test the output is as expected.

# Linting

We use [CSharpier](https://csharpier.com) to lint our code.

You can run the linter with `dotnet csharpier format .`

## License

This project is licensed under The Open Government Licence (OGL) Version 3.  
See the [LICENSE](./LICENSE) for more details.
