using System;

// http://www.codeproject.com/Articles/44166/D-FFT-of-an-Image-in-C
// LICENSE  The Code Project Open License (CPOL)

namespace AiThisAndThat.math.fft
{
    struct ComplexNumber
    {
        public double real, imag;

        public ComplexNumber(double real, double imagenary)
        {
            this.real = real;
            this.imag = imagenary;
        }
        public float Magnitude()
        {
            return (float)System.Math.Sqrt(real * real + imag * imag);
        }
        public float Phase()
        {
            System.Diagnostics.Debug.Assert(real != 0.0);

            return (float)System.Math.Atan(imag / real);
        }

        public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
        {
            ComplexNumber result;

            result.real = a.real*b.real - a.imag*b.imag;
            result.imag = a.imag*a.real + a.real*b.imag;

            return result;
        }

        public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
        {
            ComplexNumber result;

            result.real = a.real + b.real;
            result.imag = a.imag + b.imag;

            return result;
        }
    }
    
    class FFT
    {
        public enum EnumDirection
        {
            FORWARD,
            BACKWARD
        }

        int nx, ny;                      //Number of Points in Width & height
        int Width, Height;
        ComplexNumber[,] Fourier;              //Fourier Magnitude  Array Used for Inverse FFT
        public ComplexNumber[,] Output;        // FFT Normal
        public double[,] inverseResult;

        
        /// <summary>
        /// Constructor for Inverse FFT
        /// </summary>
        /// <param name="Input"></param>
        
        public FFT(ComplexNumber[,] Input)
        {
            nx = Width = Input.GetLength(0);
            ny = Height = Input.GetLength(1);
            Fourier = Input;

        }

        /** \brief constructor for non inverse input
         * 
         * 
         */
        public FFT(double[,] input)
        {
            int x, y;

            nx = Width = input.GetLength(0);
            ny = Height = input.GetLength(1);

            Fourier = new ComplexNumber[Width, Height];

            for( y = 0; y < input.GetLength(1); y++ )
            {
                for( x = 0; x < input.GetLength(0); x++ )
                {
                    Fourier[x, y].real = input[x, y];
                }
            }
        }



        /// <summary>
        /// Calculate Fast Fourier Transform of Input Image
        /// </summary>
        public void doFft(EnumDirection direction)
        {
            int directionAsInt;

            if( direction == EnumDirection.FORWARD )
            {
                directionAsInt = 1;
            }
            else
            {
                directionAsInt = -1;
            }

            //Initializing Fourier Transform Array
            //int i,j;
            /****Fourier =new ComplexNumber [Width,Height];****/
            Output = new ComplexNumber[Width, Height];
            //Copy Image Data to the Complex Array
            /***for (i=0;i<=Width -1;i++)
                for (j = 0; j <= Height - 1; j++)
                {
                    Fourier[i, j].real =(double) GreyImage[i, j];
                    Fourier[i, j].imag = 0;
                }***/
            //Calling Forward Fourier Transform
            Output = fft2d(Fourier, nx, ny, direction);

            if( direction == EnumDirection.BACKWARD )
            {
                int x, y;

                inverseResult = new double[Width, Height];

                for( y = 0; y < Height; y++ )
                {
                    for( x = 0; x < Width; x++ )
                    {
                        inverseResult[x, y] = Output[x, y].Magnitude();
                    }
                }
            }
            else
            {
                inverseResult = null;
            }

            return;
        }

        /**
         *  Perform a 2D FFT inplace given a complex 2D array
         *  The direction dir, 1 for forward, -1 for reverse
         *  The size of the array (nx,ny)
         *  Return false if there are memory problems or
         *  the dimensions are not powers of 2
         */
        public static ComplexNumber [,] fft2d(ComplexNumber[,] c, int nx, int ny, EnumDirection direction)
          {
            int i,j;
            int m;//Power of 2 for current number of points
            double []real;
            double []imag;
            ComplexNumber [,] output;//=new COMPLEX [nx,ny];
            output = c; // Copying Array
            // Transform the Rows 
            real = new double[nx] ;
            imag = new double[nx];
            
            for (j=0;j<ny;j++) 
            {
              for (i=0;i<nx;i++) 
               {
                 real[i] = c[i,j].real;
                 imag[i] = c[i,j].imag;
               }
            // Calling 1D FFT Function for Rows
              m = (int)System.Math.Log((double)nx, 2);//Finding power of 2 for current number of points e.g. for nx=512 m=9
            fft1d(direction,m,ref real,ref imag);

              for (i=0;i<nx;i++) 
               {

                   output[i, j].real = real[i];
                   output[i, j].imag = imag[i];
               }
            }
            // Transform the columns  
            real = new double[ny];
            imag = new double[ny];
                  
            for (i=0;i<nx;i++) 
            {
              for (j=0;j<ny;j++) 
               {

                   real[j] = output[i, j].real;
                   imag[j] = output[i, j].imag;
               }
           // Calling 1D FFT Function for Columns
              m = (int)System.Math.Log((double)ny, 2);//Finding power of 2 for current number of points e.g. for nx=512 m=9
           fft1d(direction,m,ref real,ref imag);
             for (j=0;j<ny;j++) 
               {
                output[i, j].real = real[j];
                output[i, j].imag = imag[j];
               }
            }
          
            return(output);
        }
        /*-------------------------------------------------------------------------
            This computes an in-place complex-to-complex FFT
            x and y are the real and imaginary arrays of 2^m points.
            dir = 1 gives forward transform
            dir = -1 gives reverse transform
            Formula: forward
                     N-1
                      ---
                    1 \         - j k 2 pi n / N
            X(K) = --- > x(n) e                  = Forward transform
                    N /                            n=0..N-1
                      ---
                     n=0
            Formula: reverse
                     N-1
                     ---
                     \          j k 2 pi n / N
            X(n) =    > x(k) e                  = Inverse transform
                     /                             k=0..N-1
                     ---
                     k=0
            */
        public static void fft1d(EnumDirection direction, int m, ref double[] x, ref double[] y )
            {
                long nn, i, i1, j, k, i2, l, l1, l2;
                double c1, c2, tx, ty, t1, t2, u1, u2, z;
                /* Calculate the number of points */
                nn = 1;
                for (i = 0; i < m; i++)
                    nn *= 2;
                /* Do the bit reversal */
                i2 = nn >> 1;
                j = 0;
                for (i = 0; i < nn - 1; i++)
                {
                    if (i < j)
                    {
                        tx = x[i];
                        ty = y[i];
                        x[i] = x[j];
                        y[i] = y[j];
                        x[j] = tx;
                        y[j] = ty;
                    }
                    k = i2;
                    while (k <= j)
                    {
                        j -= k;
                        k >>= 1;
                    }
                    j += k;
                }
                /* Compute the FFT */
                c1 = -1.0;
                c2 = 0.0;
                l2 = 1;
                for (l = 0; l < m; l++)
                {
                    l1 = l2;
                    l2 <<= 1;
                    u1 = 1.0;
                    u2 = 0.0;
                    for (j = 0; j < l1; j++)
                    {
                        for (i = j; i < nn; i += l2)
                        {
                            i1 = i + l1;
                            t1 = u1 * x[i1] - u2 * y[i1];
                            t2 = u1 * y[i1] + u2 * x[i1];
                            x[i1] = x[i] - t1;
                            y[i1] = y[i] - t2;
                            x[i] += t1;
                            y[i] += t2;
                        }
                        z = u1 * c1 - u2 * c2;
                        u2 = u1 * c2 + u2 * c1;
                        u1 = z;
                    }
                    c2 = System.Math.Sqrt((1.0 - c1) / 2.0);
                    if (direction == EnumDirection.FORWARD)
                        c2 = -c2;
                    c1 = System.Math.Sqrt((1.0 + c1) / 2.0);
                }
                /* Scaling for forward transform */
                if (direction == EnumDirection.FORWARD)
                {
                    for (i = 0; i < nn; i++)
                    {
                        x[i] /= (double)nn;
                        y[i] /= (double)nn;
                       
                    }
                }

                return;
            }
        
    }
}
