﻿using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJob
    {
        /// <summary>
        /// Main work loop of the class.
        /// </summary>
        void Run(object state, bool timedOut);
    }
}