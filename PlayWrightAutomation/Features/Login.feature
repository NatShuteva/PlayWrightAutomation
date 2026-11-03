Feature: Login

Scenario Outline: Successful login
  Given the login page is open
  When valid credentials are entered for "<Username>" and "<Password>"
  Then the profile page is displayed

Examples:
  | Username  | Password    |
  | NatashaQA | Tester1234$ |
  | Natasha1  | Natasha123# |

Scenario Outline: Invalid login
	Given the login page is open
	When valid credentials are entered for "<Username>" and "<Password>"
	Then the profile page is not displayed

Examples:
	| Username  | Password    |
	| NatashaQA | Tester12345 |

Scenario Outline: Logout
	Given the login page is open
	When valid credentials are entered for "<Username>" and "<Password>"
    Then the profile page is displayed
    When click on the Logout button
    Then the login page is displayed
Examples:
	| Username  | Password    |
	| NatashaQA | Tester1234$ |