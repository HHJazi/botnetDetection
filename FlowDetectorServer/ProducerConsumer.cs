// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProducerConsumer.cs" company="RandomSkunk">
// Copyright © 2011 by Brian Friesen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Biotracker.Client.ProcessMonitor
{
    /// <summary>
    /// A class to synchronize between producers and a consumer.
    /// </summary>
    /// <typeparam name="T">
    /// The type of item to be produced and consumed.
    /// </typeparam>
    public class ProducerConsumer<T>
    {
        /// <summary>
        /// The <see cref="Action{T}"/> that is executed in the consumer thread.
        /// </summary>
        private readonly Action<T> consumerAction;

        /// <summary>
        /// Whether <see cref="Enqueue"/> should add data items when <see cref="IsRunning"/> is false.
        /// </summary>
        private readonly bool enqueueWhenStopped;

        /// <summary>
        /// Whether to call <see cref="Clear"/> when <see cref="IsRunning"/> is set to false.
        /// </summary>
        private readonly bool clearQueueUponStop;

        /// <summary>
        /// The <see cref="Queue{T}"/> that contains the data items.
        /// </summary>
        private readonly Queue<T> queue = new Queue<T>();

        /// <summary>
        /// Synchronizes access to <see cref="queue"/>.
        /// </summary>
        private readonly object queueLocker = new object();

        /// <summary>
        /// Allows the consumer thread to block when no items are available in the <see cref="queue"/>.
        /// </summary>
        private readonly AutoResetEvent queueWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Prevents more than one thread from modifying <see cref="IsRunning"/> at a time.
        /// </summary>
        private readonly object isRunningLocker = new object();

        /// <summary>
        /// Allows the consumer thread to block when <see cref="IsRunning"/> is false.
        /// </summary>
        private readonly AutoResetEvent isRunningWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Whether the consumer thread is processing data items.
        /// </summary>
        private volatile bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerConsumer{T}"/> class.
        /// </summary>
        /// <param name="consumerAction">
        /// The <see cref="Action{T}"/> that will be executed when the consumer thread processes a data item.
        /// </param>
        /// <param name="enqueueWhenStopped">
        /// Whether <see cref="Enqueue"/> should add data items when <see cref="IsRunning"/> is false.
        /// </param>
        /// <param name="clearQueueUponStop">
        /// Whether to call <see cref="Clear"/> when <see cref="IsRunning"/> is set to false.
        /// </param>
        /// <param name="startImmediately">
        /// Whether to start the consumer thread immediately.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="consumerAction"/> is null.
        /// </exception>
        public ProducerConsumer(Action<T> consumerAction, bool enqueueWhenStopped = true, bool clearQueueUponStop = false, bool startImmediately = true)
        {
            if (consumerAction == null)
            {
                throw new ArgumentNullException("consumerAction");
            }

            this.consumerAction = consumerAction;
            this.enqueueWhenStopped = enqueueWhenStopped;
            this.clearQueueUponStop = clearQueueUponStop;

            this.isRunning = startImmediately;

            new Thread(this.ConsumeItems) { IsBackground = true }.Start();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the consumer thread is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }

            set
            {
                // Allow only one thread at a time to modify IsRunning.
                lock (this.isRunningLocker)
                {
                    if (value == this.isRunning)
                    {
                        return;
                    }

                    if (value)
                    {
                        // Make sure queueWaitHandle is in a non-signalled state (so it will block) before signalling isRunningWaitHandle.
                        this.queueWaitHandle.Reset();

                        // Also make sure to set isRunning to true before signalling isRunningWaitHandle.
                        this.isRunning = true;

                        // Make sure to signal isRunningWaitHandle AFTER we are sure that queueWaitHandle is non-signalled and isRunning is true.
                        this.isRunningWaitHandle.Set();
                    }
                    else
                    {
                        // Make sure isRunningWaitHandle is in a non-signalled state (so it will block) BEFORE setting isRunning to false or signalling queueWaitHandle.
                        this.isRunningWaitHandle.Reset();

                        // Make sure to set isRunning to false AFTER we are sure isRunningWaitHandle is non-signalled (will block).
                        this.isRunning = false;

                        // Make sure to signal queueWaitHandle AFTER we are sure that isRunningWaitHandle is non-signalled (will block), and isRunning is set to false.
                        this.queueWaitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Start the consumer thread.
        /// </summary>
        public void Start()
        {
            this.IsRunning = true;
        }

        /// <summary>
        /// Stop the consumer thread.
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;
        }

        /// <summary>
        /// Clear all data items from the queue.
        /// </summary>
        public void Clear()
        {
            lock (this.queueLocker)
            {
                this.queue.Clear();
            }
        }

        /// <summary>
        /// Enqueue a data item.
        /// </summary>
        /// <param name="item">
        /// The data item to enqueue.
        /// </param>
        public void Enqueue(T item)
        {
            lock (this.queueLocker)
            {
                // If we're running, or we should queue items up when we're stopped...
                if (this.isRunning || this.enqueueWhenStopped)
                {
                    // ...queue up the item...
                    this.queue.Enqueue(item);

                    // ...and signal the consumer thread.
                    this.queueWaitHandle.Set();
                }
            }
        }

        /// <summary>
        /// The consumer thread.
        /// </summary>
        private void ConsumeItems()
        {
            while (true)
            {
                if (this.isRunning)
                {
                    T nextItem = default(T);

                    // Later on, we'll need to know whether there was an item in the queue.
                    bool doesItemExist;

                    lock (this.queueLocker)
                    {
                        doesItemExist = this.queue.Count > 0;
                        if (doesItemExist)
                        {
                            nextItem = this.queue.Dequeue();
                        }
                    }

                    if (doesItemExist)
                    {
                        // If there was an item in the queue, process it...
                        this.consumerAction(nextItem);
                    }
                    else
                    {
                        // ...otherwise, wait for the an item to be queued up.
                        this.queueWaitHandle.WaitOne();
                    }
                }
                else
                {
                    if (this.clearQueueUponStop)
                    {
                        // We have just stopped, so clear the queue if we're configured to do so.
                        this.Clear();
                    }

                    // Wait to start up again.
                    this.isRunningWaitHandle.WaitOne();
                }
            }
        }
    }
}
