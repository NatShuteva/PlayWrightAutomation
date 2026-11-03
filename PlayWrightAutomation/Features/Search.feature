Feature: Search

Scenario Outline: Search book by title
  Given the Book Store page is open
  When enter text book in the search field "<Title>"
  Then the search results are displayed

Examples:
	| Title            |
	| Git Pocket Guide |
	| Learning JavaScript Design Patterns |

Scenario: Click on book title and get the details of the book
    Given the Book Store page is open
	When click on book title from the filtered book
	Then book details are displayed correctly
    When click on the Back To Book Store button 
    Then the Book Store page is opened
