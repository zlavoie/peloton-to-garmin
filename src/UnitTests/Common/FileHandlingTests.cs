﻿using Common;
using Common.Dto.Garmin;
using FluentAssertions;
using Moq.AutoMock;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace UnitTests.Common
{
	public class FileHandlingTests
	{
		private static readonly string DataDirectory = Path.Join(Directory.GetCurrentDirectory(), "..", "..", "..", "Data");
		private static readonly string DeviceInfoPath = Path.Join(DataDirectory, "deviceInfo.xml");
		private static readonly string BadDeviceInfoPath = Path.Join(DataDirectory, "badDeviceInfo.xml");

		[Test]
		public void TryDeserializeXml_Deserializes()
		{
			var autoMocker = new AutoMocker();
			IOWrapper fileHandler = autoMocker.CreateInstance<IOWrapper>();

			bool success = fileHandler.TryDeserializeXml<GarminDeviceInfo>(DeviceInfoPath, out GarminDeviceInfo deserialized);
			
			success.Should().BeTrue();
			deserialized.Should().NotBeNull();
		}

		[Test]
		public void TryDeserializeXml_ReturnsFalse_ForInvalidFile()
		{
			var autoMocker = new AutoMocker();
			IOWrapper fileHandler = autoMocker.CreateInstance<IOWrapper>();

			bool success = fileHandler.TryDeserializeXml<GarminDeviceInfo>(BadDeviceInfoPath, out GarminDeviceInfo deserialized);

			success.Should().BeFalse();
			deserialized.Should().BeNull();
		}

		[Test]
		public void TryDeserializeXml_Validate_Async_ReadAccess()
		{
			var autoMocker = new AutoMocker();
			IOWrapper fileHandler = autoMocker.CreateInstance<IOWrapper>();

			Parallel.For(0, 10, (itr, loopState) =>
			{
				bool success = fileHandler.TryDeserializeXml<GarminDeviceInfo>(DeviceInfoPath, out GarminDeviceInfo deserialized);
				success.Should().BeTrue();
				deserialized.Should().NotBeNull();
			});
		}
	}
}
