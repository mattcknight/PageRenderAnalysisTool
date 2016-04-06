USE [Test]
GO

/****** Object:  Table [dbo].[EDHIDExceptionDetails]    Script Date: 07/30/2012 14:06:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[EDHIDExceptionDetails](
	[SeqNum] [int] NOT NULL,
	[ExceptionMessage] [varchar](1000) NOT NULL,
	[ExceptionSource] [varchar](100) NOT NULL,
	[ExceptionType] [varchar](100) NOT NULL,
	[LandingUrl] [varchar](300) NULL,
	[PageCode] [smallint] NOT NULL,
	[MachineId] [varchar](50) NULL,
	[ServerId] [tinyint] NOT NULL,
	[IsPastSubscriber] [bit] NULL,
	[IsSubscriber] [bit] NULL,
	[SubscribedInThisSession] [bit] NULL,
	[FileServer] [varchar](50) NULL,
	[BrowserType] [varchar](50) NULL,
	[BrowserLanguage] [varchar](50) NULL,
	[BrowserVersion] [varchar](50) NULL,
	[UserId] [int] NOT NULL,
	[LogEntryDateTime] [datetime] NOT NULL,
	[LogEntryId] [int] NOT NULL,
	[WebServer] [varchar](128) NOT NULL,
 CONSTRAINT [PK_EDHIDExceptionDetails] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


