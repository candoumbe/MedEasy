# MedEasy

An open source healthcare management system.


<ul type="number">
    <li><a href='#lnk-why'>Why ?</a></li>
    <li><a href='#lnk-how'>How it works</a></li>
    <li><a href='#lnk-services'>Available services</a></li>
    <ul>
        <li><a href='#lnk-services-proxy'>Reverse proxy</a></li>
        <li><a href='#lnk-services-agenda'>Agenda API</a></li>
        <li><a href='#lnk-services-documents'>Documents API</a></li>
        <li><a href='#lnk-services-identity'>Identity API</a></li>
        <li><a href='#lnk-services-identity'>Measures API</a></li>
        <li><a href='#lnk-services-identity'>Patients API</a></li>
    </ul>
    <li><a href='#lnk-get-started'>Get started</a></li>
    <li><a href='#lnk-contribute'>Want to contribute</a></li>
    <li><a href='#lnk-troubleshooting'>Troubleshooting</a></li>
</list>

## <a id="lnk-why">Why ?</a>

Why not ?


## <a id="lnk-how">How it works</a>

MedEasy works as a set of [independant services](#lnk-services) that operates together.

**Design principles**
1. each service should work independantly from others
2. each service owns its data : data are never shared by two services.
3. services can be updated independantly from one an other.
4. [HATEAOS](https://en.wikipedia.org/wiki/HATEOAS) all the way !
5. services must be storage agnostic meaning they should never rely on storage specific features.

### <a id="lnk-services">Available services</a>

#### <a id="lnk-services-agenda">Agenda API</a>
`ReverseProxy` handles all incoming requests and hides the remaining services from the public network.
It handles compression, caching, authentication, authorization on the behalf of all other services.

#### <a id="lnk-services-agenda">Agenda API</a>
`Agenda API` handles appointments and attendees.

#### <a id="lnk-services-documents">Documents API</a>
`Documents API` handles file storage/retrieval.

#### <a id="lnk-services-identity">Identity API</a>
`Identity API` handles [`accounts`](/src/services/identity/Identity.DTO/AccountInfo.cs) that can be used to log into the
application. Relies heavily on JWT

#### <a id="lnk-services-measures">Measures API</a>
`Measures API` handles all sorts of physiological measures :
- [`blood pressure`](/src/services/measures/Measures.DTO/BloodPressureInfo.cs)
- [`body weight`](/src/services/measures/Measures.DTO/BodyWeightInfo.cs)


#### <a id="lnk-services-patients">Patients API</a>
`Patients API` handles patients primary data (name, date of birth, etc.)

## <a id="lnk-get-started">Get started</a>

1. Clone this repo
2. Install [Nuke tool](https://www.nuget.org/packages/Nuke.GlobalTool/) globally.
this tool is used to perform various tasks (migrations, running tests, etc.). 
3. Install [Tye tool](https://www.nuget.org/packages/Microsoft.Tye) locally.

You should be good to go !

Run `nuke --help` to see all available options

## <a id="lnk-contribute">Want to contribute ?</a>

You can start contributing by looking at [`good first issues`](https://github.com/candoumbe/MedEasy/contribute) 
on the issue tracker.

Make sure you've read the [contribution guidelines](CONTRIBUTING.md)

## <a id="lnk-contribute">Troubleshooting</a>
If you find an issue you can submit a pull request (PRs are welcome 😀 !!) or [open an issue](https://github.com/candoumbe/MedEasy/issues/new). 
