USE [Test]
GO

/****** Object:  Table [dbo].[EDHIDPageStacks]    Script Date: 07/30/2012 14:07:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[EDHIDPageStacks](
	[SeqNum] [int] NOT NULL,
	[PageStack] [varchar](max) NULL,
 CONSTRAINT [PK_EDHIDPageStacks] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[EDHIDPageStacks]  WITH NOCHECK ADD  CONSTRAINT [FK_EDHIDPageStacks_EDHIDExceptionDetails] FOREIGN KEY([SeqNum])
REFERENCES [dbo].[EDHIDExceptionDetails] ([SeqNum])
GO

ALTER TABLE [dbo].[EDHIDPageStacks] CHECK CONSTRAINT [FK_EDHIDPageStacks_EDHIDExceptionDetails]
GO


