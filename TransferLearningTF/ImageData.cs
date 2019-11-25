﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace TransferLearningTF
{
    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
}
