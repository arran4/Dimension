﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Folder : FSListing
    {
        public Folder[] folderChildren;
        public File[] fileChildren;
    }
}
