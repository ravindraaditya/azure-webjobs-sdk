﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.TestCommon;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Host.UnitTests.Hosting
{
    public class DrainModeManagerTests
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly TestLoggerProvider _loggerProvider;

        public DrainModeManagerTests()
        {
            _loggerFactory = new LoggerFactory();
            _loggerProvider = new TestLoggerProvider();
            _loggerFactory.AddProvider(_loggerProvider);
        }

        [Fact]
        public void RegisterListener_AddsToListenerCollection()
        {
            Mock<IListener> listener = new Mock<IListener>(MockBehavior.Strict);
            var drainModeManager = new DrainModeManager(_loggerFactory);
            drainModeManager.RegisterListener(listener.Object);

            Assert.Equal(drainModeManager.Listeners.ElementAt(0), listener.Object);
        }


        [Fact]
        public async Task RegisterListener_EnableDrainModeAsync_CallsStopAsyncAndEnablesDrainMode()
        {
            Mock<IListener> listener = new Mock<IListener>(MockBehavior.Strict);
            listener.Setup(bl => bl.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));

            var drainModeManager = new DrainModeManager(_loggerFactory);
            drainModeManager.RegisterListener(listener.Object);

            await drainModeManager.EnableDrainModeAsync();
            listener.VerifyAll();

            Assert.Equal(drainModeManager.DrainModeEnabled, true);

            Assert.Collection(_loggerProvider.GetAllLogMessages().Select(p => p.FormattedMessage),
               p => Assert.Equal("DrainMode is set to True", p),
               p => Assert.Equal("Calling StopAsync on the registered listeners", p),
               p => Assert.Equal("Call to StopAsync complete, registered listeners are now stopped", p));
        }
    }
}