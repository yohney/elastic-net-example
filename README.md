# The Purpose
This is an example of how Elastic Search can be integrated easily with .NET application. Feel free to fork/comment if you like.

# The Structure
Solution consists of several projects:

* Elastic.Example.Data - Defines EF entities for the project
* Elastic.Example.Services - Contains SearchService class, and Indexing/Mapping classes
* Elastic.Example.Tests - already set-up tests using Effort library to encourage TDD
* Elastic.Example.TMDB.Console - obtain an API key from TMDB and fetch the data yourself (the limit on API is 40 requests/sec), so you might get impatient soon
* Elastic.Example.Indexing.Console - run this to index all the data you pulled from TMDB into ES
* Elastic.Example.Search.Console - try querying on data when you have it all mapped and indexed
