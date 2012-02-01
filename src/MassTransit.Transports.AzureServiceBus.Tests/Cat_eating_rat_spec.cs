﻿// Copyright 2011 Henrik Feldt
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using Magnum.Extensions;
using Magnum.TestFramework;
using MassTransit.TestFramework;
using MassTransit.Transports.AzureServiceBus.Configuration;
using NUnit.Framework;

namespace MassTransit.Transports.AzureServiceBus.Tests
{
	public class Cat_eating_rat_spec
		: given_two_buses
	{
		ConsumerOf<Rat> the_cat_is;
		Future<Rat> cat_having_dinner;
			
		[When]
		public void sending_to_remote_bus()
		{
			cat_having_dinner = new Future<Rat>();
			the_cat_is = new ConsumerOf<Rat>(a_large_rat_actually =>
				{
					Console.WriteLine("Miaooo!!!");
					Console.WriteLine(a_large_rat_actually.Sound + "!!!");
					Console.WriteLine("Cat: chase! ...");
					Console.WriteLine("*silence*");
					Console.WriteLine("Cat: *Crunch chrunch*");
					cat_having_dinner.Complete(a_large_rat_actually);
				});

			RemoteBus.SubscribeInstance(the_cat_is);
			
			RemoteBus.ShouldHaveSubscriptionFor<Rat>();
			LocalBus.ShouldHaveSubscriptionFor<Rat>();

			var cat = LocalBus.GetEndpoint(new Uri(string.Format("azure-sb://{0}/remote_bus", AccountDetails.Namespace)));
			cat.Send<Rat>(new { Sound = "mzmmzzzzz" });
		}

		[Then]
		public void the_rat_got_eaten()
		{
			cat_having_dinner
				.WaitUntilCompleted(8.Seconds())
				.ShouldBeTrue();
		}
	}

	public interface Rat
	{
		string Sound { get; set; }
	}
}