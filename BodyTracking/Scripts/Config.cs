using Unity.Barracuda;

namespace mj.gist.tracking.body {
    public class Config {
        public int Stride { get; private set; }
        public int InputWidth { get; private set; }
        public int InputHeight { get; private set; }
        public int OutputWidth { get; private set; }
        public int OutputHeight { get; private set; }

        public TensorShape InputShape => new TensorShape(1, InputHeight, InputWidth, 3);
        public int InputFootPrint => InputWidth * InputHeight * 3;

        public Config(ResourceSet resourceSet, int width, int height) {
            Stride = resourceSet.stride;
            InputWidth = (width + 15) / 16 * 16 + 1;
            InputHeight = (height + 15) / 16 * 16 + 1;
            OutputWidth = InputWidth / Stride + 1;
            OutputHeight = InputHeight / Stride + 1;
        }
    }
}