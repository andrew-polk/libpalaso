using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using SIL.DblBundle.Tests.Properties;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.TestUtilities;

namespace SIL.DblBundle.Tests
{
	[TestFixture]
	class DblMetadataTests
	{
		private DblTextMetadata<DblMetadataLanguage> _metadata;
		private DblTextMetadata<DblMetadataLanguage> _metadata2;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			var xs = new XmlSerializer(typeof(DblTextMetadata<DblMetadataLanguage>));
			using (TextReader reader = new StringReader(Resources.AcholiMetadataVersion1_5_xml))
				_metadata = (DblTextMetadata<DblMetadataLanguage>)xs.Deserialize(reader);

			using (TextReader reader = new StringReader(Resources.AcholiMetadataVersion2_1_xml))
				_metadata2 = (DblTextMetadata<DblMetadataLanguage>)xs.Deserialize(reader);
		}

		[Test]
		public void GetVersion()
		{
			Assert.AreEqual("1.5", _metadata.Version);
			Assert.AreEqual("2.1", _metadata2.Version);
		}

		[Test]
		public void GetTypeVersion()
		{
			Assert.AreEqual("1.5", _metadata.TypeVersion);
			Assert.AreEqual("2.1", _metadata2.TypeVersion);
		}

		[Test]
		public void GetId()
		{
			Assert.AreEqual("3b9fdc679b9319c3", _metadata.Id);
			Assert.AreEqual("3b9fdc679b9319c3", _metadata2.Id);
		}

		[Test]
		public void GetName()
		{
			Assert.AreEqual("Acholi New Testament 1985", _metadata.Identification.Name);
			Assert.AreEqual("Acholi New Testament 1985", _metadata2.Identification.Name);
		}

		[Test]
		public void GetParatextSystemId()
		{
			Assert.AreEqual("3b9fdc679b9319c3ee45ab86cc1c0c42930c2979", _metadata.Identification.SystemIds.FirstOrDefault(sid => sid.Type.Equals("paratext")).Id);
			Assert.AreEqual("3b9fdc679b9319c3ee45ab86cc1c0c42930c2979", _metadata2.Identification.SystemIds.FirstOrDefault(sid => sid.Type.Equals("paratext")).Id);
		}

		[Test]
		public void GetLanguageIso()
		{
			Assert.AreEqual("ach", _metadata.Language.Iso);
			Assert.AreEqual("ach", _metadata2.Language.Iso);
		}

		[Test]
		public void GetCopyrightStatement()
		{
			const string expectedValue = @"<p>© 1985 The Bible Society of Uganda</p>";
			Assert.AreEqual(expectedValue, _metadata.Copyright.Statement.Xhtml);
			Assert.AreEqual("xhtml", _metadata.Copyright.Statement.ContentType);

			Assert.AreEqual(expectedValue, _metadata2.Copyright.Statement.Xhtml);
			Assert.AreEqual("xhtml", _metadata2.Copyright.Statement.ContentType);

			Assert.AreEqual(expectedValue, _metadata.Copyright.FullStatement.StatementContent.Xhtml);
			Assert.AreEqual("xhtml", _metadata.Copyright.FullStatement.StatementContent.Type);

			Assert.AreEqual(expectedValue, _metadata2.Copyright.FullStatement.StatementContent.Xhtml);
			Assert.AreEqual("xhtml", _metadata2.Copyright.FullStatement.StatementContent.Type);
		}

		[Test]
		public void GetPromoVersionInfo()
		{
			const string expectedValue = @"<h1>Acholi New Testament 1985</h1><p>This translation, published by the Bible Society " +
				@"of Uganda, was first published in 1985.</p><p>If you are interested in obtaining a printed copy, please contact " +
				@"the Bible Society of Uganda at <a href=""http://www.biblesociety-uganda.org/"">www.biblesociety-uganda.org</a>.</p>";
			Assert.AreEqual(expectedValue, _metadata.Promotion.PromoVersionInfo.Xhtml);
			Assert.AreEqual("xhtml", _metadata.Promotion.PromoVersionInfo.ContentType);

			Assert.AreEqual(expectedValue, _metadata2.Promotion.PromoVersionInfo.Xhtml);
			Assert.AreEqual("xhtml", _metadata2.Promotion.PromoVersionInfo.ContentType);
		}

		[Test]
		public void GetDateArchived()
		{
			Assert.AreEqual("2014-05-28T15:18:31.080800", _metadata.ArchiveStatus.DateArchived);
			Assert.AreEqual("2014-05-28T15:18:31.080800", _metadata2.ArchiveStatus.DateArchived);
		}

		[Test]
		public void IsTextReleaseBundle()
		{
			Assert.True(_metadata.IsTextReleaseBundle);
			Assert.True(_metadata2.IsTextReleaseBundle);
		}

		[Test]
		public void Load_Successful()
		{
			using (var metadataFile = new TempFile())
			{
				File.WriteAllText(metadataFile.Path, Resources.metadata_xml);
				Exception exception;
				DblTextMetadata<DblMetadataLanguage>.Load<DblTextMetadata<DblMetadataLanguage>>(metadataFile.Path, out exception);
				Assert.Null(exception);
			}
		}

		[Test]
		public void Load_FileDoesNotExist_HandlesException()
		{
			Exception exception;
			DblTextMetadata<DblMetadataLanguage>.Load<DblTextMetadata<DblMetadataLanguage>>("", out exception);
			Assert.NotNull(exception);
		}

		[TestCase("1.5", false)]
		[TestCase("2.1", true)]
		public void Serialize(string versionNumber, bool version2)
		{
			var metadata = new DblTextMetadata<DblMetadataLanguage>
			{
				Id = "myid",
				Revision = 1,
				Identification = new DblMetadataIdentification
				{
					Name = "myname",
					SystemIds = new HashSet<DblMetadataSystemId> { new DblMetadataSystemId { Type = "mytype", Id = "mysystemidid" } }
				},
				Promotion = new DblMetadataPromotion
				{
					PromoVersionInfo = new DblMetadataXhtmlContentNode { Xhtml = @"<h1>Acholi New Testament 1985</h1><p>More text</p>" },
				},
				ArchiveStatus = new DblMetadataArchiveStatus { DateArchived = "dateArchived" }
			};
			if (version2)
			{
				metadata.Version = versionNumber;
				metadata.Copyright = new DblMetadataCopyright
				{
					FullStatement = new DblMetadataCopyrightFullStatement
					{
						StatementContent = new DblMetadataXhtmlContentNodeWithType { Xhtml = "<p>© 1985 The Bible Society of Uganda</p>" }
					}
				};
			}
			else
			{
				metadata.TypeVersion = versionNumber;
				metadata.Copyright = new DblMetadataCopyright
				{
					Statement = new DblMetadataXhtmlContentNode { Xhtml = "<p>© 1985 The Bible Society of Uganda</p>" }
				};
			}

			string expectedResult =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<DBLMetadata id=""myid"" version=""{0}"" revision=""1"">
	<identification>
		<name>myname</name>
		<systemId type=""mytype"">
			<id>mysystemidid</id>
		</systemId>
	</identification>
	<copyright>
		<fullStatement>
			<statementContent type=""xhtml"">
				<p>© 1985 The Bible Society of Uganda</p>
			</statementContent>
		</fullStatement>
	</copyright>
	<promotion>
		<promoVersionInfo contentType=""xhtml"">
			<h1>Acholi New Testament 1985</h1>
			<p>More text</p>
		</promoVersionInfo>
	</promotion>
	<archiveStatus>
		<dateArchived>dateArchived</dateArchived>
	</archiveStatus>
</DBLMetadata>";

			expectedResult = string.Format(expectedResult, versionNumber);

			AssertThatXmlIn.String(expectedResult).EqualsIgnoreWhitespace(metadata.GetAsXml());
		}
	}
}
