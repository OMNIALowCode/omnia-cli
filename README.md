# OMNIA Low-code Platform CLI

A command-line client for OMNIA Low-code Platform.

## Installation

Download and install the [.NET Core 3.1 SDK](https://www.microsoft.com/net/download) or newer. Once installed, run the following command:

```bash
dotnet tool install --global omnia.cli
```


## Authentication

Before using the OMNIA Platform CLI, you will need to [create an API Client](https://docs.omnialowcode.com/omnia3_apiclienttutorial.html#3-define-an-api-client). 

Make sure you provide access to that API Client when using commands related to a Tenant.

The API Client will be needed to configure the Tool and will be saved in the User AppData.


## Usage

```text
Usage: omnia-cli [options] [command]

Options:
  --version     Show version information
  -?|-h|--help  Show help information

```

### Application / Import command

To import data to a tenant you can use the following command, providing the path to an Excel file. The Excel file must follow a set of rules that you can see below.

`omnia-cli application import --subscription MY_SUBSCRIPTION --tenant MY_TENANT --path "PATH_TO_FILE"`

#### Setting up the Excel file

**Index sheet**
The import will use the first sheet as an index of the structure in the file.
You will need to layout, in the first sheet, a mapping between sheets and the entities you want to import.

This configuration is a table where you define 3 columns:

  - Sheet: *Name of the sheet.*
  - Entity: *Name of the application concept to which you want to import the sheet.*
  - DataSource: *The data source of the defined entity. You can leave it empty for Default System data source.*

Example:

| Sheet | Entity   | DataSource |
| ----- | -------- | ---------- |
| A     | Customer |            |
| B     | Products | MyERP      |
| C     | Contacts | MyCRM      |

**Attribute mapping**

The Name of each attribute that you want to import should be the column name in the first row of the sheet.

**Collection attributes**

In case you are importing a collection of a given entity, you will need to represent the master/detail structure.
You can do that by adding an entry to the index where the *Entity* specifies the Parent of the entity using the structure `PARENT.CHILD_ATTRIBUTE`.

Example:

| Sheet | Entity             | DataSource |
| ----- | ------------------ | ---------- |
| A     | Customer           |            |
| A     | Customer.Addresses |            |

To relate the collection data to the parent, you will also need to identify the parent entries using an `#ID` column.
Then, you can correlate the data by using a column with the name `#ParentID` in the child datasheet.

## Contributing

See contribution Guidelines [here](CONTRIBUTING.md).

## License

OMNIA CLI is available under the [MIT license](http://opensource.org/licenses/MIT).
