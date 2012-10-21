﻿using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using Microsoft.Practices.TransientFaultHandling;

namespace NHibernate.SqlAzure
{
    /// <summary>
    /// NHibernate client driver for SQL Azure that includes transient fault-handling.
    /// </summary>
    public class SqlAzureClientDriver : ReliableSql2008ClientDriver
    {
        protected override ReliableSqlConnection CreateReliableConnection()
        {
            var retryStrategies = new List<RetryStrategy>();
            const string incremental = "Incremental Retry Strategy";
            const string interval = "Fixed Interval Retry Strategy";
            const string backoff = "Backoff Retry Strategy";
            retryStrategies.Add(new Incremental(incremental, 10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)));
            retryStrategies.Add(new FixedInterval(interval, 10, TimeSpan.FromSeconds(1)));
            retryStrategies.Add(new ExponentialBackoff(backoff, 10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10), false));

            var retryManager = new RetryManagerImpl(retryStrategies, interval, backoff, incremental, interval, interval, interval);

            var connection = new ReliableSqlConnection(null, retryManager.GetDefaultSqlConnectionRetryPolicy(), retryManager.GetDefaultSqlCommandRetryPolicy());
            connection.ConnectionRetryPolicy.Retrying += RetryEventHandler();
            connection.CommandRetryPolicy.Retrying += RetryEventHandler();
            return connection;
        }

        /// <summary>
        /// An event handler delegate which will be called on connection and command retries.
        /// </summary>
        /// <returns>A custom method for handling the retry events</returns>
        protected virtual EventHandler<RetryingEventArgs> RetryEventHandler()
        {
            return null;
        }
    }
}