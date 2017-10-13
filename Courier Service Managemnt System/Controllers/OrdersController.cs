using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using MvcWebApiTask;
using MvcWebApiTask.Models;
using System.Web.Script.Serialization;
using GoogleMaps.LocationServices;

namespace MvcWebApiTask.Controllers
{
    public class OrdersController : ApiController
    {
        private demoEntities db = new demoEntities();

        public double GetLat(string address)
        {

            var locationService = new GoogleLocationService();
            var point = locationService.GetLatLongFromAddress(address);

            var latitude = point.Latitude;
            var longitude = point.Longitude;
            return latitude;
        }

        public double GetLong(string address)
        {

            var locationService = new GoogleLocationService();
            var point = locationService.GetLatLongFromAddress(address);

            var latitude = point.Latitude;
            var longitude = point.Longitude;
            return longitude;
        }


        public double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            double distance = 0;

            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                        + Math.Cos(lat2) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //Calculate radius of earth
            // For this you can assume any of the two points.
            double radiusE = 6378135; // Equatorial radius, in metres
            double radiusP = 6356750; // Polar Radius

            //Numerator part of function
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            //Denominator part of the function
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                            + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);

            //Calaculate distance in metres.
            distance = radius * c;
            return distance;
        }


        [HttpGet]
        public string GetIp()
        {
            using (WebClient c = new WebClient())
            {
                try
                {
                    var ip= c.DownloadString("https://wtfismyip.com/text");
                    
                    string url = string.Format("http://api.ipinfodb.com/v3/ip-city/?key=46ccfb79195bb5f5cf8f0b969fb7d3d6fd6db8e331430742ba1abef8aa7e0f49&ip={0}&format=json", ip);
                    string json = c.DownloadString(url);
                    LocationIp location = new JavaScriptSerializer().Deserialize<LocationIp>(json);
                    List<LocationIp> locations = new List<LocationIp>();
                    locations.Add(location);

                    return location.Longitude + " ," +location.Latitude + " " + location.CountryCode+" " + location.CountryName + " " + location.RegionName + " " + location.CityName;
                }
                catch (Exception e)
                {
                    return "not valid" + e.Message;
                }
            }
        }

        [HttpGet]
        [Route("api/Orders/CheckStatus")]
        public string CheckStatus(int id)
        {

            Order o = db.Orders.Find(id);
            o = db.Orders.Where(i => i.OrderId == id).FirstOrDefault();
            if (o.Status == true)
            {
                return "Delievered";

            }
            else
            {
                return "Not Delievered";
            }
        }

        [HttpGet]
        [Route("api/Orders/CheckTotalAmt")]
        public double CheckTotalAmt(int id)
        {
            Order o = db.Orders.Find(id);
            o = db.Orders.Where(i => i.OrderId == id).FirstOrDefault();

            if (o.Distance != 0)
            {
                return o.TotalAmount = o.Distance * 100;
            }
            return 0;
        }

        [HttpGet]
        [Route("api/Orders/CheckCurrentLoc")]
        public string CheckCurrentLoc(int id)
        {
            Order o = db.Orders.Find(id);
            o = db.Orders.Where(i => i.OrderId == id).FirstOrDefault();
            if (id != 0)
            {
                o.CurrentLocation = GetIp();
                return o.CurrentLocation;
                //db.Entry(o).State = EntityState.Modified;
                //db.SaveChanges();
                //return "success";
            }
            return "not";
        }

        public double TotalAmt(double dis, string dri)
        {
            Order o = new Order();

            if (dri == "Flight")
            {
                return o.TotalAmount = dis * 0.20;
            }
            else
            {
                return o.TotalAmount = dis * 0.10;
            }
        }

        [HttpGet]
        [Route("api/Orders/GetLocs1")]
        public double GetLocs1(string ad)
        {
            Order o = new Order();

            double s = GetLat(ad);
            return s;
        }

        [HttpGet]
        [Route("api/Orders/GetLocs2")]    
        public double GetLocs2(string ad)
        {
            Order o = new Order();

            double s = GetLong(ad);
            return s;
        }

        [HttpGet]
        [Route("api/Orders/PostOrder")]
        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            order = new Order();
            order.OrderName = "Test";          
            order.OrderAddress = "Karachi";
            order.OrderFrom = "Suleman";
            order.OrderTo = "Ahmed";
            order.DestinationFrom = "Kemari karachi Pakistan";
            order.DestinationTo = "Bahadurabad karachi Pakistan";
            double ad1 = GetLocs1(order.DestinationFrom);
            double ad2 = GetLocs2(order.DestinationFrom);
            double ad3 = GetLocs1(order.DestinationTo);
            double ad4 = GetLocs2(order.DestinationTo);
            order.CurrentLocation = GetIp(); //this will insert the current location of device.
            order.Distance = GetDistanceBetweenPoints(ad1, ad2, ad3, ad4);
            order.PaymentMode = "Before Delievery";
            order.DrivingMode = "Flight";
            order.TotalAmount = TotalAmt(order.Distance,order.DrivingMode); //this will insert Total Amount from Distance.
            order.Status = false;
            db.Orders.Add(order);
            db.SaveChanges();

            return Ok("success");
        }

        //Geocoder geo = new Geocoder("AIzaSyBagP2MJ32KVgbCWCv5o1bA_dEYo9wqhhEs");

        //public string DesGeo(string des)
        //{
        //    var loc = geo.Geocode(des);
        //    return des;
        //}

        // GET: api/Orders
        //public IQueryable<Order> GetOrders()
        //{
        //    return db.Orders;
        //}

        //// GET: api/Orders/5
        //[ResponseType(typeof(Order))]
        //public IHttpActionResult GetOrder(int id)
        //{
        //    Order order = db.Orders.Find(id);
        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(order);
        //}

        // PUT: api/Orders/5
        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutOrder(int id, Order order)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != order.OrderId)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(order).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!OrderExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //[HttpGet]
        //[Route("api/Orders/findDis")]
        //public double findDis()
        //{
        //    //Order order = new Order();
        //   return GetLang("Kemari karachi Pakistan");
        //    //string desFrom = DesGeo(order.DestinationFrom); //this will convert the address to GSP Coordinates.
        //    //order.DestinationTo = "Bahadurabad karachi Pakistan";
        //    //var desTo = DesGeo(order.DestinationTo); //this will convert the address to GSP Coordinates.

        //    //return s;
        //    //order.Distance = GetDistanceBetweenPoints(desFrom.First(), desFrom.Last(), desTo.First(), desTo.First());
        //    //return order.Distance;
        //}

        //// DELETE: api/Orders/5
        //[ResponseType(typeof(Order))]
        //public IHttpActionResult DeleteOrder(int id)
        //{
        //    Order order = db.Orders.Find(id);
        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Orders.Remove(order);
        //    db.SaveChanges();

        //    return Ok(order);
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        //private bool OrderExists(int id)
        //{
        //    return db.Orders.Count(e => e.OrderId == id) > 0;
        //}

        //var locationService = new GoogleLocationService();
        //var point1 = locationService.GetLatLongFromAddress("Kemari Karachi Sindh Pakistan");
        //double latitude1 = point1.Latitude;
        //double longitude1 = point1.Longitude;
        //var point2 = locationService.GetLatLongFromAddress("Bahadurabad Karachi Sindh Pakistan");
        //double latitude2 = point1.Latitude;
        //double longitude2 = point1.Longitude;

    }
}