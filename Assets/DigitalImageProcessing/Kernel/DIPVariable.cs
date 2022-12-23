
namespace DIP
{
    public static partial class ImageProcessing
    {
        #region  Variables
        public enum TextureChannel { R = 0, G, B, Gay };//In HSV Space TextureChannel { H = 0, S, V, Gay }
        public enum KernelType { average = 0, gaussian, laplacian, prewitt, sobel };
        public enum FilterType { Arithemetic = 0, Geometric, Harmonic, Contraharmonic, Median, Max, Min, MidPoint, AlphaTrim };
        public enum Boundary_Option { zero = 0, replicate, symmetric };
        public enum Nosise_Type { uniform = 0, guassian, pepperSalt, logNormal, exponential, rayleigh, erland };
        public enum Morphological_Operator { Erosion = 0, Dilation, Opening, Closing, HMT };
        public enum Image_Type { BlackWhite = 0, Grayscale };
        public enum Neighborhood_type { Four = 0, Eight };
        public enum SortDirection { CW = 0, CCW };
        public enum Conner { LBottom = 0, LUp, RUp, RBottom };
        public enum Accuracy { Precision = 0, Approximate };
        public enum Laplacian_Display { Abs = 0, Shift, Positive };
        public enum Edge_Method { Sobel=0,Prewitt,LoG,Canny };
        public enum Edge_Operator { Sobel = 0, Prewitt};
        public enum CheckDir {Horizontal =0,Vertical,Both }
        #endregion

       
    }

}
