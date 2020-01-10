# OMNIA Low-code Platform CLI

A command-line client for OMNIA Low-code Platform.

## Installation

Download and install the [.NET Core 2.1 SDK](https://www.microsoft.com/net/download) or newer. Once installed, run the following command:

```bash
dotnet tool install --global omnia.cli
```


## Authentication

Before using the OMNIA Platform CLI, you will need to [create an API Client](https://docs.omnialowcode.com/omnia3_apiclienttutorial.html#3-define-an-api-client). 

Be sure that you give access to that API Client when using commands related to a Tenant.

The API Client will be needed to configure the Tool and will be saved in saved in the User AppData.


## Usage

```text
Usage: omnia-cli [options] [command]

Options:
  --version     Show version information
  -?|-h|--help  Show help information

```


## Contributing

See contribution Guidelines [here](CONTRIBUTING.md).

## License

OMNIA CLI is available under the [MIT license](http://opensource.org/licenses/MIT).
