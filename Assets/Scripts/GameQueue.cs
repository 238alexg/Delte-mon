/*
 *	Battle Delts
 *	GameQueue.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using BattleDelts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts 
{
	public class GameQueue 
	{
        public static GameQueue Inst;

        Queue<QueueItem> WorldQueue = new Queue<QueueItem>();
        Queue<QueueItem> BattleQueue = new Queue<QueueItem>();
        Queue<QueueItem> ImmediateQueue = new Queue<QueueItem>();
        Stack<QueueItem> UIQueueItemPool = new Stack<QueueItem>();

        MonoBehaviour CoroutineParent;
        Coroutine QueueWorker;
        QueueType CurrentQueueType;

        public GameQueue(MonoBehaviour couroutineParent)
        {
            if (Inst != null)
            {
                throw new System.Exception("GameQueue already has an instance!");
            }
            Inst = this;
            CoroutineParent = couroutineParent;
            QueueWorker = CoroutineParent.StartCoroutine(QueueWorkerRoutine());
            CurrentQueueType = QueueType.World;
        }

        public static void Add(QueueType type, string message = null, System.Action action = null, IEnumerator enumerator = null)
        {
            Inst.VerifyWorkingQueue();

            QueueItem queueItem = Inst.GetNewQueueItem();
            queueItem.Populate(message, action, enumerator);
            Inst.GetQueue(type).Enqueue(queueItem);
        }

        public static void BattleAdd(string message = null, System.Action action = null, IEnumerator enumerator = null)
        {
            Add(QueueType.Battle, message, action, enumerator);
        }

        public static void WorldAdd(string message = null, System.Action action = null, IEnumerator enumerator = null)
        {
            Add(QueueType.World, message, action, enumerator);
        }

        public static void QueueImmediate(string message = null, System.Action action = null, IEnumerator enumerator = null)
        {
            Inst.VerifyWorkingQueue();

            QueueItem queueItem = Inst.GetNewQueueItem();
            queueItem.Populate(message, action, enumerator);
            Inst.ImmediateQueue.Enqueue(queueItem);
        }

        public void ChangeQueueType(QueueType type)
        {
            CurrentQueueType = type;
        }

        QueueItem GetNewQueueItem()
        {
            return UIQueueItemPool.Count == 0 ? new QueueItem() : UIQueueItemPool.Pop();
        }

        void VerifyWorkingQueue()
        {
            if (QueueWorker == null)
            {
                QueueWorker = CoroutineParent.StartCoroutine(QueueWorkerRoutine());
            }
        }

        IEnumerator QueueWorkerRoutine()
        {
            while(true)
            {
                QueueItem queueItem = GetNextQueueItem();

                if (queueItem != null)
                {
                    if (queueItem.Message != null)
                    {
                        // Print message
                        yield return CoroutineParent.StartCoroutine(UIManager.Inst.displayMessage(queueItem.Message));
                        // Wait for message to finish
                        yield return new WaitUntil(() => UIManager.Inst.endMessage);

                        UIManager.Inst.endMessage = false;
                        UIManager.Inst.MessageUI.SetActiveIfChanged(false);
                    }
                    if (queueItem.Action != null)
                    {
                        queueItem.Action();
                    }
                    if (queueItem.Enumerator != null)
                    {
                        yield return CoroutineParent.StartCoroutine(queueItem.Enumerator);
                    }
                }

                yield return null;
            }
        }

        QueueItem GetNextQueueItem()
        {
            QueueItem queueItem;
            if (ImmediateQueue.Count > 0)
            {
                queueItem = ImmediateQueue.Dequeue();
            }
            else
            {
                Queue<QueueItem> queue = GetQueue(CurrentQueueType);
                if (queue.Count > 0)
                {
                    return queue.Dequeue();
                }
            }
            return null;
        }

        Queue<QueueItem> GetQueue(QueueType type)
        {
            switch (type)
            {
                case QueueType.World: return WorldQueue;
                case QueueType.Battle: return BattleQueue;
                default: throw new System.NotImplementedException("Trying to get a queue type that doesn't exist: " + type);
            }
        }

        public class QueueItem
        {
            public string Message = null;
            public System.Action Action = null;
            public IEnumerator Enumerator = null;

            public void Populate(string message = null, System.Action action = null, IEnumerator enumerator = null)
            {
                Message = message;
                Action = action;
                Enumerator = enumerator;
            }

            public void Clear()
            {
                Message = null;
                Action = null;
                Enumerator = null;
            }
        }

        public enum QueueType
        {
            World,
            Battle
        }
    }
}