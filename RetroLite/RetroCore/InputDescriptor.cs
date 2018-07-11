using LibRetro.Types;

namespace RetroLite.RetroCore
{
    public class InputDescriptor
    {
        private uint _port;
        private RetroDevice _device;
        private uint _index;
        private uint _id;

        private string _description;

        public InputDescriptor(uint port, RetroDevice device, uint index, uint id, string description)
        {
            _port = port;
            _device = device;
            _index = index;
            _id = id;
            _description = description;
        }
    }
}