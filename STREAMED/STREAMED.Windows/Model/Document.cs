﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STREAMED
{
  class Document
  {
    [SQLite.AutoIncrement, SQLite.PrimaryKey, SQLite.Indexed]
    public int id { get; set; }
    public String DocumentType { get; set; }
    public String DebitCategory { get; set; }
    public String CreditCategory { get; set; }
    public String ImagePath { get; set; }
  }
}