using System;

// modified version from C# ai1

namespace Datastructures
{
    class Vector2<Type> where Type : IComparable
    {
        public Vector2()
        {
            //this.x; // = (Type)0;
            //this.y; // = (Type)0;
        }

        public Vector2(Type x, Type y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2<Type> operator +(Vector2<Type> a, Vector2<Type> b)
        {
            Vector2<Type> result;

            result = new Vector2<Type>();
            result.x = (dynamic)a.x + (dynamic)b.x;
            result.y = (dynamic)a.y + (dynamic)b.y;

            return result;
        }

        public static Vector2<Type> operator -(Vector2<Type> a, Vector2<Type> b)
        {
            Vector2<Type> result;

            result = new Vector2<Type>();
            result.x = (dynamic)a.x - (dynamic)b.x;
            result.y = (dynamic)a.y - (dynamic)b.y;

            return result;
        }

        public Type magnitude()
        {
            dynamic xDynamic = x;
            dynamic yDynamic = y;

            return (Type)System.Math.Sqrt(xDynamic * xDynamic + yDynamic * yDynamic);
        }

        public void scale(Type value)
        {
            dynamic xDynamic;
            dynamic yDynamic;

            xDynamic = x;
            yDynamic = y;

            xDynamic *= value;
            yDynamic *= value;

            x = xDynamic;
            y = yDynamic;
        }

        public Vector2<Type> getScaled(Type value)
        {
            Vector2<Type> result;

            dynamic xDynamic;
            dynamic yDynamic;

            xDynamic = x;
            yDynamic = y;

            xDynamic *= value;
            yDynamic *= value;

            result = new Vector2<Type>();
            result.x = xDynamic;
            result.y = yDynamic;

            return result;
        }

        public Vector2<Type> normalized()
        {
            Type magnitudeValue;
            Type invMagnitude;

            magnitudeValue = magnitude();
            invMagnitude = (dynamic)1.0f / magnitudeValue;

            return getScaled(invMagnitude);
        }

        public Vector2<Type> clone()
        {
            Vector2<Type> result;

            result = new Vector2<Type>();
            result.x = x;
            result.y = y;

            return result;
        }


        static public Vector2<Type> min(Vector2<Type> a, Vector2<Type> b, Vector2<Type> c, Vector2<Type> d)
        {
            dynamic xDynamic;
            dynamic yDynamic;
            Vector2<Type> result;

            xDynamic = a.x;
            yDynamic = a.y;

            if( b.x < xDynamic )
            {
                xDynamic = b.x;
            }

            if (b.y < yDynamic)
            {
                yDynamic = b.y;
            }


            if (c.x < xDynamic)
            {
                xDynamic = c.x;
            }

            if (c.y < yDynamic)
            {
                yDynamic = c.y;
            }

            if (d.x < xDynamic)
            {
                xDynamic = d.x;
            }

            if (d.y < yDynamic)
            {
                yDynamic = d.y;
            }

            result = new Vector2<Type>();
            result.x = xDynamic;
            result.y = yDynamic;

            return result;
        }

        static public Vector2<Type> min(Vector2<Type> a, Vector2<Type> b)
        {
            Type xResult;
            Type yResult;
            Vector2<Type> result;

            xResult = a.x;
            yResult = a.y;

            if (b.x.CompareTo(xResult) < 0)
            {
                xResult = b.x;
            }

            if (b.y.CompareTo(yResult) < 0)
            {
                yResult = b.y;
            }

            result = new Vector2<Type>();
            result.x = xResult;
            result.y = yResult;

            return result;
        }

        static public Vector2<Type> max(Vector2<Type> a, Vector2<Type> b, Vector2<Type> c, Vector2<Type> d)
        {
            dynamic xDynamic;
            dynamic yDynamic;
            Vector2<Type> result;

            xDynamic = a.x;
            yDynamic = a.y;

            if (b.x > xDynamic)
            {
                xDynamic = b.x;
            }

            if (b.y > yDynamic)
            {
                yDynamic = b.y;
            }


            if (c.x > xDynamic)
            {
                xDynamic = c.x;
            }

            if (c.y > yDynamic)
            {
                yDynamic = c.y;
            }

            if (d.x > xDynamic)
            {
                xDynamic = d.x;
            }

            if (d.y > yDynamic)
            {
                yDynamic = d.y;
            }

            result = new Vector2<Type>();
            result.x = xDynamic;
            result.y = yDynamic;

            return result;
        }

        static public Vector2<Type> max(Vector2<Type> a, Vector2<Type> b)
        {
            Type xResult;
            Type yResult;
            Vector2<Type> result;

            xResult = a.x;
            yResult = a.y;

            if (b.x.CompareTo(xResult) > 0)
            {
                xResult = b.x;
            }

            if (b.y.CompareTo(yResult) > 0)
            {
                yResult = b.y;
            }

            result = new Vector2<Type>();
            result.x = xResult;
            result.y = yResult;

            return result;
        }

        public void normalize()
        {
            dynamic magnitudeResult;

            magnitudeResult = magnitude();

            x /= magnitudeResult;
            y /= magnitudeResult;
        }

        public Vector2<Type> getTangent()
        {
            Vector2<Type> result;

            result = new Vector2<Type>();
            result.x = y;
            result.y = -(dynamic)x;

            return result;
        }

        public Type dot(Vector2<Type> other)
        {
            return x * (dynamic)other.x + y * (dynamic)other.y;
        }

        public Type x;
        public Type y;
    }
}
