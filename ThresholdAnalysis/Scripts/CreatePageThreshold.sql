USE [MatchMetrics]
GO


CREATE TABLE [dbo].[PageThresholdPage](
	[PageId] [int] NOT NULL,
	[Page] [varchar](2000) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PageId] ASC
)
) 

CREATE TABLE [dbo].[PageThresholdDataSourceAlias](
	[DataSourceAliasId] [int] NOT NULL,
	[DataSourceAlias] [varchar](2000) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DataSourceAliasId] ASC
)
) 

CREATE TABLE [dbo].[PageThresholdProcName](
	[ProcNameId] [int] NOT NULL,
	[ProcName] [sysname] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProcNameId] ASC
)
) 


CREATE TABLE [dbo].[PageThresholdServerName](
	[ServerNameId] [int] NOT NULL,
	[ServerName] [sysname] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ServerNameId] ASC
)
)

GO



CREATE TABLE [dbo].[PageThresholdHeader](
	[SeqNum] [int] NOT NULL,
	[PageId] [int] NULL,
	[LogEntryDateTime] [datetime] NOT NULL,
	[LogEntryId] [int] NOT NULL,
	[WebServer] [varchar](128) NOT NULL,
	[UserId] [int] NOT NULL,
	[HasLoginAuth] [bit] NOT NULL,
	[Sid] [uniqueidentifier] NOT NULL,
	[IsSubscriber] [bit] NOT NULL,
	[ExecutionTime] [bigint] NOT NULL,
	[DbTime] [bigint] NOT NULL,
	[ComputedDbExecTime] [bigint] NULL,
 CONSTRAINT [PK_PageThresholdHeader3] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC
)
)

GO



ALTER TABLE [dbo].[PageThresholdHeader]  WITH NOCHECK ADD FOREIGN KEY([PageId])
REFERENCES [dbo].[PageThresholdPage] ([PageId])
GO

CREATE TABLE [dbo].[PageThresholdDetail](
	[SeqNum] [int] NOT NULL,
	[ProcSeq] [int] NOT NULL,
	[ProcNameId] [int] NOT NULL,
	[DataSourceAliasId] [int] NOT NULL,
	[PageId] [int] NULL,
	[LogEntryDateTime] [datetime] NOT NULL,
	[ExecutionTime] [bigint] NOT NULL,
	[BeginDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[ComputedExecTime] [bigint] NULL,
	[ServerNameId] [int] NULL,
	[Duration] [bigint] NULL
 CONSTRAINT [PK_PageThresholdDetail3_1] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC,
	[ProcSeq] ASC
)
) 

GO

ALTER TABLE [dbo].[PageThresholdDetail]  WITH NOCHECK ADD FOREIGN KEY([DataSourceAliasId])
REFERENCES [dbo].[PageThresholdDataSourceAlias] ([DataSourceAliasId])
GO

ALTER TABLE [dbo].[PageThresholdDetail]  WITH NOCHECK ADD FOREIGN KEY([PageId])
REFERENCES [dbo].[PageThresholdPage] ([PageId])
GO

ALTER TABLE [dbo].[PageThresholdDetail]  WITH NOCHECK ADD FOREIGN KEY([ProcNameId])
REFERENCES [dbo].[PageThresholdProcName] ([ProcNameId])
GO

CREATE TABLE [dbo].[PageThresholdHeaderHourlySummary](
	[LogEntryDate] [date] NOT NULL,
	[HourOfDay] [tinyint] NOT NULL,
	[PageId] [int] NOT NULL,
	[DC][tinyint] NOT NULL,
	[ExecutionTime] [bigint] NOT NULL,
	[DbTime] [bigint] NOT NULL,
	[CntExecutions] [bigint] NOT NULL,
	[PrevHr_ExecutionTime] [bigint] NULL,
	[PrevHr_DbTime] [bigint] NULL,
	[PrevHr_CntExecutions] [bigint] NULL,
	[Yesterday_ExecutionTime] [bigint] NULL,
	[Yesterday_DbTime] [bigint] NULL,
	[Yesterday_CntExecutions] [bigint] NULL,
	[WoW_ExecutionTime] [bigint] NULL,
	[WoW_DbTime] [bigint] NULL,
	[WoW_CntExecutions] [bigint] NULL,
	[NWoW_ExecutionTime] [bigint] NULL,
	[NWoW_DbTime] [bigint] NULL,
	[NWoW_CntExecutions] [bigint] NULL
) 

ALTER TABLE [dbo].[PageThresholdHeaderHourlySummary] ADD  CONSTRAINT [PK_PageThresholdHeaderHourlySummary] PRIMARY KEY CLUSTERED 
(
	[LogEntryDate] ASC,
	[HourOfDay] ASC,
	[PageId] ASC,
	[DC] ASC
)
GO

CREATE TABLE [dbo].[ProcThresholdHourlySummary](
	[LogEntryDate] [date] NOT NULL,
	[HourOfDay] [tinyint] NOT NULL,
	[ProcNameId] [int] NOT NULL,
	[DataSourceAliasId] [int] NOT NULL,
	[DC] [tinyint] NOT NULL,
	[CntExecutions] [bigint] NOT NULL
) 

ALTER TABLE [dbo].[ProcThresholdHourlySummary] ADD  CONSTRAINT [PK_ProcThresholdHourlySummary] 
PRIMARY KEY CLUSTERED 
(
	[LogEntryDate] ASC,
	[HourOfDay] ASC,
	[ProcNameId] ASC,
	[DataSourceAliasId] ASC,
	[DC] ASC
)
GO