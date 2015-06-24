﻿using System;
using System.Collections.Generic;
using Ledger;

namespace TestsDomain
{
	public class CandidateMemento : ISequenced
	{
		public int SequenceID { get; set; }

		public string Name { get; set; }
		public List<string> Emails { get; set; }
	}
}