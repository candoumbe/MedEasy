Feature: Agenda
	Handle the hassle related to start/end dates and timezones.

@Acceptance
Scenario: Add attendee to an appointment
	Given an existing appointment for a specific user
	When I create a new appointment
	Then The appointment should have an attendee called "Bruce Wayne"