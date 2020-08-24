using System;
using TechTalk.SpecFlow;

namespace Agenda.AcceptanceTests.Steps
{
    [Binding]
    public class AppointmentSteps
    {
        [Given(@"An existing appointment with no attendee")]
        public void GivenAnExistingAppointmentWithNoAttendee()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I add an attendee called ""(.*)""")]
        public void WhenIAddAnAttendeeCalled(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"The appointment should have an attendee called ""(.*)""")]
        public void ThenTheAppointmentShouldHaveAnAttendeeCalled(string p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
