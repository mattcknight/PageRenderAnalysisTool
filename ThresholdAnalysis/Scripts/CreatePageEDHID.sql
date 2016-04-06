USE [MatchMetrics]
GO

/****** Object:  Table [dbo].[PageTimeOut]    Script Date: 02/06/2013 13:32:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING OFF
GO

CREATE TABLE [dbo].[PageEDHIDStackTrace](
	[StackTraceId][int] NOT NULL,
	[StackTrace][varchar](max) NOT NULL,
CONSTRAINT[PK_PageEDHIDStackTrace_StackTraceId] PRIMARY KEY CLUSTERED
(
	[StackTraceId] ASC
)
)

CREATE TABLE [dbo].[PageEDHIDMessageText](
	[MessageTextId][int] NOT NULL,
	[MessageText][varchar](max) NOT NULL,
CONSTRAINT [PK_PageEDHIDMessageText_MessageTextId] PRIMARY KEY CLUSTERED
(
	[MessageTextId] ASC
)
)


CREATE TABLE [dbo].[PageEDHID](
	[SeqNum] [int] NOT NULL,
	[LogEntryDateTime] [datetime] NOT NULL,
	[LogEntryId] [int] NOT NULL,
	[WebServer] [varchar](128) NOT NULL,
	[UserId] [int] NOT NULL,
	[PageCode] [int] NULL,
	[Sid][uniqueidentifier] NOT NULL,
	[IsSubscriber] [bit] NOT NULL,
	[StackTraceId] [int] NOT NULL,
	[MessageTextId] [int] NOT NULL,
 CONSTRAINT [PK_PageEDHID_SeqNum] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MatchMetricsData]
) ON [MatchMetricsData]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[PageEDHID] WITH NOCHECK ADD FOREIGN KEY([MessageTextId])
REFERENCES [dbo].[PageEDHIDMessageText]([MessageTextId])
GO

ALTER TABLE [dbo].[PageEDHID] WITH NOCHECK ADD FOREIGN KEY([StackTraceId])
REFERENCES [dbo].[PageEDHIDStackTrace]([StackTraceId])
GO