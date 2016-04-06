USE [MatchMetrics]
GO

/****** Object:  Table [dbo].[PageTimeOut]    Script Date: 02/06/2013 13:32:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING OFF
GO

CREATE TABLE [dbo].[PageStackTrace](
	[StackTraceId] [int] NOT NULL,
	[StackTrace] [varchar](2000) NOT NULL,
 CONSTRAINT [PK_PageStackTrace_StackTraceId] PRIMARY KEY CLUSTERED 
(
	[StackTraceId] ASC
)
) 

CREATE TABLE [dbo].[PageMessageDetails](
	[MessageDetailsId] [int] NOT NULL,
	[MessageDetails] [varchar](2000) NOT NULL,
 CONSTRAINT [PK_PageMessageDetails_MessageDetailsId] PRIMARY KEY CLUSTERED 
(
	[MessageDetailsId] ASC
)
) 



CREATE TABLE [dbo].[PageTimeOut](
	[SeqNum] [int] NOT NULL,
	[LogEntryDateTime] [datetime] NOT NULL,
	[LogEntryId] [int] NOT NULL,
	[WebServer] [varchar](128) NOT NULL,
	[UserId] [int] NOT NULL,
	[PageCode] [int] NULL,
	[Sid] [uniqueidentifier] NOT NULL,
	[IsSubscriber] [bit] NOT NULL,
	[TimeOutDataSourceAliasId] [int] NOT NULL,
	[StackTraceId] [int] NOT NULL,
	[MsgDetailsId] [int] NOT NULL,
 CONSTRAINT [PK_PageTimeOut_SeqNum] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MatchMetricsData]
) ON [MatchMetricsData]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[PageTimeOut]  WITH NOCHECK ADD FOREIGN KEY([MsgDetailsId])
REFERENCES [dbo].[PageMessageDetails] ([MessageDetailsId])
GO

ALTER TABLE [dbo].[PageTimeOut]  WITH NOCHECK ADD FOREIGN KEY([StackTraceId])
REFERENCES [dbo].[PageStackTrace] ([StackTraceId])
GO

ALTER TABLE [dbo].[PageTimeOut]  WITH NOCHECK ADD FOREIGN KEY([TimeOutDataSourceAliasId])
REFERENCES [dbo].[PageThresholdDataSourceAlias] ([DataSourceAliasId])
GO

CREATE TABLE [dbo].[PageTimeoutHourlySummary](
	[LogEntryDate] [date] NOT NULL,
	[HourOfDay] [tinyint] NOT NULL,
	[TimeOutDataSourceAliasId] [int] NOT NULL,
	[DC][tinyint] NOT NULL,
	[CntTimeouts] [bigint] NOT NULL,
) 

ALTER TABLE [dbo].[PageTimeoutHourlySummary] ADD  CONSTRAINT [PK_PageTimeoutHourlySummary] PRIMARY KEY CLUSTERED 
(
	[LogEntryDate] ASC,
	[HourOfDay] ASC,
	[TimeOutDataSourceAliasId] ASC,
	[DC] ASC
)

