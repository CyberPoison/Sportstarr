﻿using NzbDrone.Api.REST;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;

namespace NzbDrone.Api
{
    public abstract class NzbDroneRestModuleWithSignalR<TResource, TModel> : NzbDroneRestModule<TResource>, IHandle<ModelEvent<TModel>>
        where TResource : RestResource, new()
        where TModel : ModelBase, new()
    {
        private readonly IBroadcastSignalRMessage _signalRBroadcaster;

        protected NzbDroneRestModuleWithSignalR(IBroadcastSignalRMessage signalRBroadcaster)
        {
            _signalRBroadcaster = signalRBroadcaster;
        }

        protected NzbDroneRestModuleWithSignalR(IBroadcastSignalRMessage signalRBroadcaster, string resource)
            : base(resource)
        {
            _signalRBroadcaster = signalRBroadcaster;
        }

        public void Handle(ModelEvent<TModel> message)
        {
            if (message.Action == ModelAction.Deleted || message.Action == ModelAction.Sync)
            {
                BroadcastResourceChange(message.Action);
            }

            BroadcastResourceChange(message.Action, message.Model.Id);
        }

        protected void BroadcastResourceChange(ModelAction action, int id)
        {
            var resource = GetResourceById(id);
            BroadcastResourceChange(action, resource);
        }


        protected void BroadcastResourceChange(ModelAction action, TResource resource)
        {
            var signalRMessage = new SignalRMessage
            {
                Name = Resource,
                Body = new ResourceChangeMessage<TResource>(resource, action)
            };

            _signalRBroadcaster.BroadcastMessage(signalRMessage);
        }

     
        protected void BroadcastResourceChange(ModelAction action)
        {
            var signalRMessage = new SignalRMessage
            {
                Name = Resource,
                Body = new ResourceChangeMessage<TResource>(action)
            };

            _signalRBroadcaster.BroadcastMessage(signalRMessage);
        }
    }
}