using System;
using System.Threading.Tasks;

namespace TwitterSharp.Response.RLike
{
    public class Like
    {   
        /// <summary>
        /// Whether tweet is liked or not
        /// </summary>
        public bool Liked { init; get; }
    }
}