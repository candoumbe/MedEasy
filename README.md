# MedEasy

An open source healthcare management system.


<ul type="number">
    <li><a href='#lnk-why'>Why</a></li>
    <li><a href='#lnk-how'>How it works</a></li>
    <li><a href='#lnk-services'>Available services</a></li>
    <ul>
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
- each service can work independantly from others
- each service owns its data : data are never shared by two services.
- services can be updated independantly from one an other
- HATEAOS all the way !

### <a id="lnk-services">Available services</a>

#### <a id="lnk-services-agenda">Agenda API</a>
`Agenda API` handles [appointments] and attendees.


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

1. Clone the repo
2. Install [Nuke tool](https://www.nuget.org/packages/Nuke.GlobalTool/) globally.

You should be good to go !

## <a id="lnk-contribute">Want to contribute ?</a>

You can start contributing by looking at [`good first issues`](https://github.com/candoumbe/MedEasy/contribute) 
on the issue trackers.

Make sure you'

## <a id="lnk-contribute">Troubleshooting</a>
If you find an issue you can submit a pull request (I welcome PR 😀 !!) or open an issue. 
