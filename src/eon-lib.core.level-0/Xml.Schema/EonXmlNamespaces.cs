namespace Eon.Xml.Schema {

	public static class EonXmlNamespaces {

		public static class Build {

			public static class NuGet {

				public const string Default = @"urn:eon-2020:build:nuget";

			}

		}

		public static class Description {

			public const string Package = @"urn:eon-2020:description:description-package:data-flow";

		}

		public static class Configuration {

			public const string Install = @"urn:eon-2020:configuration:install";

		}

		public static class Diagnostics {

			public static class Triggers {

				public const string Default = @"urn:eon-2020:diagnostics:triggers";

			}

			public static class Logging {

				public static class Filters {

					public const string Ns = @"urn:eon-2020:diagnostics:logging:filters";

				}

			}

			public const string Default = @"urn:eon-2020:diagnostics";

		}

		public static class Device {

			public static class ProximityCard {

				public static class WinForms {

					public const string Default = @"urn:eon-2020:device:proximity-card:win-forms";

				}

				public const string Default = @"urn:eon-2020:device:proximity-card";

			}

			public const string Default = @"urn:eon-2020:device";

		}

		public static class Data {

			public static class Filters {

				public static class Criteria {

					public static class Operators {

						public const string Ns = @"urn:eon-2020:data:filters:criteria:operators";

					}

					public const string Ns = @"urn:eon-2020:data:filters:criteria";

				}

			}

			public static class Persistence {

				public const string Default = @"urn:eon-2020:data:persistence";

			}

			public static class Job {

				public const string Default = @"urn:eon-2020:data:job";

			}

			public static class Ef6 {

				public const string Default = @"urn:eon-2020:data:ef6";

			}

			public const string Default = @"urn:eon-2020:data";

		}

		public static class Dataflow {

			public const string Default = @"urn:eon-2020:data-flow";

		}

		public static class Soa {

			public const string Default = @"urn:eon-2020:soa";

			public const string Client = @"http://schemas.digital-flare.info/eon/soa/client/2013/10/";

		}

		public static class Globalization {

			public const string Default = @"http://schemas.digital-flare.info/eon/globalization/2014/08/";

		}

		public static class RabbitMq {

			public const string Default = @"urn:eon-2020:rabbit-mq";

		}

		public static class MessageFlow {

			public static class Local {

				public const string Default = @"urn:eon-2020:message-flow:local";

			}

			public const string Ns = @"urn:eon-2020:message-flow";

		}

		public static class Triggers {

			public const string Ns = @"urn:eon-2020:triggers";

		}

		public static class Job {

			public static class Http {

				public static class Server {

					public static class Filters {

						public const string Ns = @"urn:eon-2020:job:http:server:filters";

					}

				}

				public const string Ns = @"http://schemas.digital-flare.info/eon/job/http/2018/11/";

			}

			public const string Ns = @"urn:eon-2020:job";

		}

		public static class Schedule {

			public const string Ns = @"urn:eon-2020:schedule";

		}

		public static class ServiceProcess {

			public const string Ns = @"urn:eon-2020:service-process";

		}

		public static class Security {

			public static class Authentication {

				public const string Default = @"urn:eon-2020:security:authentication";

			}

			public static class Cryptography {

				public const string Default = @"urn:eon-2020:security:cryptography";

			}

			public static class Tokens {

				public const string Default = @"urn:eon-2020:security:tokens";

			}

			public const string Default = @"urn:eon-2020:security";

		}

		public static class ComponentModel {

			public static class Dependencies {

				public const string Default = @"urn:eon-2020:component-model:dependencies";

			}

			public static class Api {

				public const string Default = @"http://schemas.digital-flare.info/eon/component-model/api/2016/06/";

			}

		}

		public static class Net {

			public static class Security {

				public const string Default = @"urn:eon-2020:net:security";

			}

			public static class Http {

				public static class Security {

					public static class Authentication {

						public const string Ns = @"urn:eon-2020:net:http:security:authentication";

					}

					public const string Default = @"http://schemas.digital-flare.info/eon/net/http/security/2017/10/";

				}

				public static class Diagnostics {

					public const string Ns = @"urn:eon-2020:net:http:diagnostics";

				}

				public static class Compression {

					public const string Ns = @"urn:eon-2020:net:http:compression";

				}

				public static class OutputCache {

					public const string Ns = @"urn:eon-2020:net:http:output-cache";

				}

				public static class Server {

					public static class Filters {

						public const string Ns = @"urn:eon-2020:net:http:server:filters";

					}

					public const string Ns = @"urn:eon-2020:net:http:server";

				}

				public const string Ns = @"urn:eon-2020:net:http";

			}

			public static class Mail {

				public const string Default = @"urn:eon-2020:net:mail";

			}

		}

		public static class Metadata {

			public static class Tree {

				public const string Default = @"urn:eon-2020:metadata:tree";

			}

		}

		public static class Resources {

			public static class XResource {

				public static class Tree {

					public const string Ns = @"urn:eon-2020:resources:xresource:tree";

				}

			}

		}

		public static class Web {

			public static class WebApi {

				public static class Security {

					public const string Default = @"http://schemas.digital-flare.info/eon/web/web-api/security/2017/10/";

				}

				public const string Default = @"http://schemas.digital-flare.info/eon/web/web-api/2015/04/";

			}

			public const string Default = @"http://schemas.digital-flare.info/eon/web/2016/11/";

		}

		public static class XDataContract {

			public const string Default = @"urn:eon-2020:runtime:serialization:xdatacontract";

		}

		public static class UI {

			public const string Default = @"urn:eon-2020:ui";

		}

		public static class XAppProchost {

			public const string Default = @"http://schemas.digital-flare.info/eon/xapp-prochost/2017/03/";

		}

		public static class IO {

			public static class Ports {

				public static class SerialPort {

					public const string Default = @"urn:eon-2020:io:ports:serial-port";

				}

				public const string Default = @"http://schemas.digital-flare.info/eon/io/ports/2017/06/";

			}

			public const string Default = @"http://schemas.digital-flare.info/eon/io/2017/06/";

		}

		public static class Interaction {

			public const string Default = @"urn:eon-2020:interaction";

		}

		public static class Reflection {

			public const string Ns = @"urn:eon-2020:reflection";

		}


		public const string Core = @"urn:eon-2020:core";

	}

}