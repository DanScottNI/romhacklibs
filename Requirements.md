# Introduction #

You will need the following to download, and compile ROMHackLib:

  * Subversion Client
  * Visual Studio 2008

# Details #

For the Subversion client, I recommend [TortoiseSVN](http://tortoisesvn.tigris.org/).

For the version of Visual Studio 2008, I have developed it in Professional Edition, but any versions less than Professional should be able to compile the code (such as Express edition).

The library uses the Client Framework subset version of .NET 3.5.  If you are stuck on Visual Studio 2005, you will need to downgrade the project to 2005's project format, and rewrite any bits of the project that use LINQ (there shouldn't be that many).