AB# - A super simple A/B Testing system for ASP.net
=======

This project is no longer actively maintained, use at your own risk. The system does work pretty well but the documentaion is quite poor, sorry

Usage
----

*All examples use the razor syntax but AB# can also work with the WebForms view engine*

Lets say we have an ASP.net based site that sells widgets and we have the following title:

   &lt;h1&gt;Boring Title&lt;/h1&gt;

The purpose of the page is to get visitors to the /BuyWidget page

And you suspect the page will perfom better with 

   &lt;h1&gt;THIS IS EXCITING&lt;/h1&gt;

All you have to do to test this is just

   @if(ABSharp.Test.Begin("New exciting title","/BuyWidget")
   {
      &lt;h1&gt;THIS IS EXCITING&lt;/h1&gt;
   }
   else
   {
      &lt;h1&gt;Boring Title&lt;/h1&gt;
   }
   @(new HtmlString(ABSharp.Test.EmitJS()))

Installtion
----

1.  Create an SQL Server database

2.  Run the following SQL to create the tables:

        CREATE TABLE [dbo].[ABSharp_TestData](
	       [TestId] [nvarchar](50) NOT NULL,
	       [ConvertionUrl] [nvarchar](500) NOT NULL
        ) ON [PRIMARY]

        CREATE TABLE [dbo].[ABSharp_Convertions](
	       [Id] [int] IDENTITY(1,1) NOT NULL,
	       [TestId] [nvarchar](50) NOT NULL,
	       [UserId] [nvarchar](250) NOT NULL,
	       [ConvertTimestamp] [datetime] NOT NULL
        ) ON [PRIMARY]

        CREATE TABLE [dbo].[ABSharp_Samples](
	       [Id] [int] IDENTITY(1,1) NOT NULL,
	       [UserId] [nvarchar](250) NOT NULL,
	       [TestId] [nvarchar](50) NOT NULL,
	       [Option] [int] NOT NULL,
	       [EnteranceTimestamp] [datetime] NULL
        ) ON [PRIMARY]

        CREATE TABLE [dbo].[ABSharp_VerifiedUsers](
	       [UserId] [nvarchar](250) NOT NULL,
        CONSTRAINT [PK_ABTest_VerifiedUsers] PRIMARY KEY CLUSTERED 
        (
	       [UserId] ASC
        )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
        ) ON [PRIMARY]

3. Add a conection string pointing to the db named "ABSharp" to your web.config, 

4. Register the "ABSharp.ABSharpModule, ABSharp" http module

5. Add the ABSharp.dll to your bin directory

6. To view the results set the same connection string under the same name in the web.config file of the ABSharp.Console application

