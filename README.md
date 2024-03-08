# Prod-DDM-API

## Description

This is a simple API that handles the CRUD operations of logFiles for the Maxon Company.

## Requirements

- .NET Core 6.0

## Installation

### Clone the repository

```shell
git clone https://github.com/ddmaxon/Prod-DDM-API.git
cd Prod-DDM-API
```

### Install the dependencies

```shell
dotnet restore
```

### Run the application

```shell
dotnet run
```

## Usage

The API has the feature, to automatically search for the log-files in a defined folder.

### Endpoints

#### GET /csv/timeline
> Returns the timeline of the log-files in the defined folder.
>
> Example:
>
> ```shell
> curl -X GET "http://localhost:5000/csv/timeline" -H "accept: application/json"
> ```

#### GET /csv/history
> Returns all the log-files in the defined folder and adds some information about the file.
>
> Example:
>
> ```shell
> curl -X GET "http://localhost:5000/csv/history" -H "accept: application/json"
> ```

#### GET /csv/history/{id}
> Returns the log-file with the given id.
>
> Example:
>
> ```shell
> curl -X GET "http://localhost:5000/csv/history/1" -H "accept: application/json"
> ```


### Contributing

1. Fork it
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request
6. Wait for the review
7. Merge the pull request
8. Done

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Contact

Levin Zimmermann - Scrum Master
- [LinkedIn](https://www.linkedin.com/in/levin-zimmermann-049ab4257/)
- [Mail](mailto:levinzimmermann06@gmail.com)
- [GitHub](https://github.com/RikoxCode)

Loise Möckli - Developer
- [LinkedIn](https://www.linkedin.com/in/loise-m%C3%B6ckli-3b3b3b1b3/)
- [Mail](mailto:)
- [GitHub]()

Jana Junemann - Developer
- [LinkedIn](https://www.linkedin.com/in/jana-junemann-3b3b3b1b3/)
- [Mail](mailto:)
- [GitHub]()

Nelio Gautschi - Developer
- [LinkedIn](https://www.linkedin.com/in/nelio-gautschi-3b3b3b1b3/)
- [Mail](mailto:)
- [GitHub]()


## Acknowledgements

- [Maxon Motor](https://www.maxonmotor.com/)