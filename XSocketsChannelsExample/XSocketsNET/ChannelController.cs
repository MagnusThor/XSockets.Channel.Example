using System;
using XSockets.Core.XSocket;
using XSockets.Core.XSocket.Helpers;
using XSockets.Core.Common.Socket.Event.Interface;

namespace XSocketsChannelsExample.XSocketsNET
{
    /// <summary>
    /// Implement/Override your custom actionmethods, events etc in this real-time MVC controller
    /// </summary>
    public class ChannelController : XSocketController
    {

        public string Channel { get; set; }


        public override void OnOpened()
        {
            // Does the client has passed a channel in the connection
            this.Channel = this.HasParameterKey("channel") ? this.GetParameter("channel") :DateTime.Now.Ticks.ToString();
            // pass back the channel to the client connecting
            this.Invoke(new { channel = this.Channel, ts = DateTime.Now, client = this.ConnectionId }, "channelConnect");
        }


        public void ChangeChannel(string channel)
        {

            // Tell others on this channel that i "leave"
            this.InvokeTo(c => c.Channel == this.Channel, new { channel = this.Channel, ts = DateTime.Now, client = this.ConnectionId }, "channelLeave");

            // set the channel Property to 'channel' provided..
            this.Channel = channel;

            this.Invoke(new { channel = channel, ts = DateTime.Now, client = this.ConnectionId }, "channelChange");


            // Tell others on this channel that someone enters
            this.InvokeTo(c => c.Channel == this.Channel, new { channel = this.Channel, ts = DateTime.Now, client = this.ConnectionId }, "channelJoin");

        }

        public override void OnClosed()
        {
            // we could also notify others on the same 'channel' that this
            // client no longer is connected to the "channel"
            // We are usign the same filter as we do in the 'Dispatch Method below'
            this.InvokeTo(c => c.Channel == this.Channel, new { channel = this.Channel, ts = DateTime.Now, client = this.ConnectionId }, "channelDisconnect");
        }

        /// <summary>
        /// Dispatch will pass the IMessage to client on the "same channel"
        /// </summary>
        /// <param name="message"></param>
        public void Dispatch(IMessage message)
        {
            // filter out clients connected using 'c => c.Channel == this.Channel'
            // and pass dispatch the 'message'
            this.InvokeTo(c => c.Channel == this.Channel, message);
        }

    }
}
