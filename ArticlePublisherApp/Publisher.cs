using System;
using Microsoft.Web.Services2;
using Microsoft.Web.Services2.Messaging;
using Microsoft.Web.Services2.Addressing;
using System.Web.Services.Protocols;
using System.Xml;
using System.Collections;
using System.IO;
using System.Collections.Specialized;

namespace ArticlePublisherApp
{
	internal class Literals
	{
		static Literals()
		{
			Literals.LocalhostTCP = "soap.tcp://" + System.Net.Dns.GetHostName() + ":";
		}
		internal readonly static string LocalhostTCP;
	}


	public delegate void NewSubscriberEventHandler(string subscriberName, string ID, Uri replyTo );
	public delegate void RemoveSubscriberEventHandler( string ID);

	/// <summary>
	/// Summary description for Publisher.
	/// </summary>
	public class Publisher : SoapReceiver
	{
	
		public event NewSubscriberEventHandler NewSubscriberEvent;
		public event RemoveSubscriberEventHandler RemoveSubscriberEvent;

		public Publisher()
		{
			_subscribers = new Hashtable();
			fsw = new FileSystemWatcher();
			System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
			string folderWatch =  ((string)(configurationAppSettings.GetValue("Publish.PublishFolder", typeof(string))));
			try
			{
				fsw = new System.IO.FileSystemWatcher(folderWatch);
			}
			catch
			{
				throw new Exception("Directory '" + folderWatch + "' referenced does not exist. " +
					"Change the fileName variable or create this directory in order to run this demo.");
			}
			fsw.Filter = "*.txt";
			fsw.Created += new FileSystemEventHandler(fsw_Created);
			fsw.Changed += new FileSystemEventHandler(fsw_Created);
			fsw.EnableRaisingEvents = true;
		}

		protected void OnNewSubscriberEvent(string Name, string ID, Uri replyTo)
		{
			if (NewSubscriberEvent	!= null)
				NewSubscriberEvent(Name, ID, replyTo);

		}

		protected void OnRemoveSubscriberEvent(string ID)
		{
			if (RemoveSubscriberEvent != null)
				RemoveSubscriberEvent(ID);
		}

		private void AddSubscriber(string ID, Uri replytoAddress, string Name)
		{
			SoapSender ssend = new SoapSender(replytoAddress);
			SoapEnvelope response = new SoapEnvelope();
			response.CreateBody();
			response.Body.InnerXml = String.Format("<x:AddSubscriber xmlns:x='urn:ArticlePublisherApp:Publisher' ><notify>Name: {0} ID: {1}</notify></x:AddSubscriber>", Name, ID);
			Action act = new Action("response");
			response.Context.Addressing.Action = act;
			ssend.Send(response);
			_subscribers.Add ( ID, new Subscriber(Name,replytoAddress, ID)  );
			OnNewSubscriberEvent(Name, ID, replytoAddress);

		}

		private void RemoveSubscriber(string ID, Uri replytoAddress)
		{
			if (_subscribers.Contains(ID) )
			{
				_subscribers.Remove(ID);
				SoapSender ssend = new SoapSender(replytoAddress);
				SoapEnvelope response = new SoapEnvelope();
				response.CreateBody();
				response.Body.InnerXml = String.Format("<x:RemoveSubscriber xmlns:x='urn:ArticlePublisherApp:Publisher' ><notify>ID: {0} Removed</notify></x:RemoveSubscriber>", ID);
				Action act = new Action("response");
				response.Context.Addressing.Action = act;
				ssend.Send(response);
				OnRemoveSubscriberEvent(ID);
			}
		}

		protected override void Receive( SoapEnvelope envelope )
		{
			
			//Determine Action if no SoapAction throw exception
			Action act = envelope.Context.Addressing.Action;
			if (act == null)
				throw new SoapHeaderException("Soap Action must be set", new XmlQualifiedName());
			
			string subscriberName = String.Empty ;
			string subscriberID = String.Empty;
			switch (act.ToString().ToLower())
			{
				case "subscribe":
					//add new subscriber
					 subscriberName = envelope.SelectSingleNode ( "//name").InnerText ;
					subscriberID = System.Guid.NewGuid().ToString();
					AddSubscriber(subscriberID, envelope.Context.Addressing.From.Address.Value, subscriberName);
					break;
				case "unsubscribe":
					subscriberID = envelope.SelectSingleNode("//name") .InnerText ;
					RemoveSubscriber(subscriberID, envelope.Context.Addressing.From.Address.Value);
					break;
				default:
					break;
			}
			
		}
		
		private void fsw_Created(object sender, System.IO.FileSystemEventArgs e)
		{
			Uri uriThis =  new Uri (Literals.LocalhostTCP + "9090/Publisher" );
			// Send each subscriber a message
			foreach(object o in _subscribers)
			{
				DictionaryEntry de = (DictionaryEntry)o;
				
				Subscriber s = (Subscriber)_subscribers[de.Key];
				SoapEnvelope responseMsg = new SoapEnvelope ();

				FileStream fs = new FileStream(e.FullPath ,FileMode.Open, FileAccess.Read , FileShare.ReadWrite );
				StreamReader sr = new StreamReader(fs);
				string strContents = sr.ReadToEnd() ;
				sr.Close();
				fs.Close();

				// Set the From Addressing value
				responseMsg.Context.Addressing.From = new From ( uriThis );
				responseMsg.Context.Addressing.Action  = new Action( "notify");
				responseMsg.CreateBody();
				responseMsg.Body.InnerXml = "<x:ArticlePublished xmlns:x='urn:ArticlePublisherApp:Publisher'><notify><file>" + e.Name +"</file><contents>" + strContents + "</contents></notify></x:ArticlePublished>";

				// Send a Response Message
				SoapSender msgSender = new SoapSender (s.ReplyTo );
				msgSender.Send ( responseMsg );
			}
		}

		internal StringCollection GetSubscribers()
		{
			StringCollection coll = new StringCollection();
			foreach(Subscriber s in _subscribers)
			{
				coll.Add(String.Format("Name - {0}\t ID - {1}\t Reply To Uri {2}", s.Name,  s.ID,  s.ReplyTo.ToString()));
			}
			return coll;
		}
		private Hashtable _subscribers;
		private FileSystemWatcher fsw;

	}

	public class Subscriber
	{
		public string Name;
		public Uri ReplyTo;
		public string ID;
		public Subscriber(string name, Uri replyTo, string id)
		{
			Name = name;
			ReplyTo = replyTo;
			ID = id;
		}
	}
	}

