﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Assignment3
{
    class Server
    {
        TcpListener server = null;

        public Server(string ip, int port)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();
            StartListener();

        }

        public void StartListener()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection ....");
                    TcpClient client = server.AcceptTcpClient();
                    if (client.Connected) 
                    {
                        Console.WriteLine("Client connected");
                        Thread t = new Thread(new ParameterizedThreadStart(HandleRequest));
                        t.Start(client);
                    }
                    else
                    {
                        Console.WriteLine("client couldn't connect");
                    }
                    
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                server.Stop();

            } 
        }
            public void HandleRequest(Object obj)
            {
                TcpClient client = (TcpClient)obj;
                var stream = client.GetStream();
                
                string request = null;

                byte[] reqBytes = new byte[2048];
                int bytesRead;
                try
                {
                while ((bytesRead = stream.Read(reqBytes, 0, reqBytes.Length)) != 0)
                {
                    var memStream = new MemoryStream();
                    memStream.Write(reqBytes, 0, bytesRead);
                    request = Encoding.UTF8.GetString(memStream.ToArray());

                    Response responseObject = new Response();

                    if (request == "{}" || String.IsNullOrEmpty(request))
                    {
                        responseObject.Body = "missing body";
                        responseObject.Status = "missing method, missing date ";
                        SendResponse(responseObject, client);
                    }
                    else
                    {
                        if (IsValidJson(request))
                        {
                            Request r = System.Text.Json.JsonSerializer.Deserialize<Request>(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                            Console.WriteLine("Client: {0}, Request: {1}", Thread.CurrentThread.ManagedThreadId, r.ToString());

                            var validMethods = new string[] { "create", "read", "update", "delete", "echo" };

                            if (isValidPath(r.Path,r.Method) == false && r.Method != "echo")
                            {

                                responseObject.Body = null;
                                responseObject.Status = "4 Bad Request";

                            }

                            if (validMethods.Contains(r.Method) == false)
                            {
                                responseObject.Body = "";
                                responseObject.Status = "illegal method, ";

                            }
                            if (String.IsNullOrEmpty(r.Path) && r.Method != "echo")
                            {
                                responseObject.Body = "";
                                responseObject.Status += "missing resource, ";

                            }
                            if (IsDateUnixTime(r.Date) == false)
                            {
                                responseObject.Body = "illegal date";
                                responseObject.Status += "illegal date, ";

                            }
                            if (r.Method == "create" || r.Method == "update" || r.Method == "echo")
                            {
                                if (String.IsNullOrEmpty(r.Body))
                                {
                                    responseObject.Body = "missing body";
                                    responseObject.Status += "missing body or ";

                                }

                                if (IsValidJson(r.Body) == false)
                                {
                                    responseObject.Body = "illegal body";
                                    responseObject.Status += "illegal body";
                                }
                                if (r.Method == "echo" && String.IsNullOrEmpty(r.Body) == false)
                                {
                                    responseObject.Body = r.Body;
                                    responseObject.Status = "1 ok";
                                }
                                if (r.Method == "update" && String.IsNullOrEmpty(r.Body) == false && IsValidJson(r.Body))
                                {
                                    CategoryService categoryService = new CategoryService();
                                    Category c = System.Text.Json.JsonSerializer.Deserialize<Category>(r.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                                    string[] strList = r.Path.Split("/");

                                    if (strList.ElementAtOrDefault(3) != null)
                                    {
                                        if (categoryService.UpdateCategory(strList[3], c) == null)
                                        {
                                            responseObject.Status = "5 not found";
                                        }
                                        else
                                        {
                                            categoryService.UpdateCategory(strList[3], c);
                                            responseObject.Status = "3 updated";
                                            responseObject.Body = CategoryService.ToJson(categoryService.UpdateCategory(strList[3], c));
                                        }
                                    } 
                                                          
                                 
                                }
                                if (r.Method =="create" && String.IsNullOrEmpty(r.Body) == false && IsValidJson(r.Body) && r.Path == "/api/categories")
                                {
                                    CategoryService categoryService = new CategoryService();
                                    var newCategory = System.Text.Json.JsonSerializer.Deserialize<Category>(r.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                                    var response = categoryService.CreateCategory(newCategory.Name);
                                    responseObject.Body = CategoryService.ToJson(response);
                                    responseObject.Status = "2 Created";
                                }


                            }

                            
                            if (r.Method == "read" && isValidPath(r.Path,r.Method))
                            {
                                if (r.Method == "read" && r.Path == "/api/categories")
                                {
                                    CategoryService categoryService = new CategoryService();
                                    var categories = categoryService.GetCategories();
                                    responseObject.Status = "1 Ok";
                                    responseObject.Body = CategoryService.ToJson(categories);
                                }
                                else
                                {
                                    string[] strList = r.Path.Split("/");
                                    CategoryService categoryService = new CategoryService();
                                    var category = categoryService.GetCategoryByID(strList[3]);

                                    if (category == null)
                                    {
                                        responseObject.Status = "5 not found";
                                    }
                                    else
                                    {
                                        responseObject.Status = "1 Ok";
                                    }
                                    responseObject.Body = CategoryService.ToJson(category);
                                }
                               
                                
                                
                            }
                            if (r.Method == "delete" && isValidPath(r.Path, r.Method))
                            {                               
                                string[] strList = r.Path.Split("/");
                                CategoryService categoryService = new CategoryService();
                                var categoryToDelete = categoryService.DeleteCategory(strList[3]);
                                if (categoryToDelete == null)
                                {
                                    responseObject.Status = "5 not found";
                                }
                                else
                                {
                                    responseObject.Status = "1 Ok";
                                }
                            }

                            SendResponse(responseObject, client); 
                        }
                        else
                        {
                            Console.WriteLine("Not valid json, Request: {0}", request);
                            responseObject.Body = "please make your request in valid json format";
                            responseObject.Status = "4 Bad Request";
                            SendResponse(responseObject, client);
                        }
                    }
                }
            }
                catch (Exception e)
                {
               
                    Console.WriteLine("Exception: {0}", e.ToString());                  
                    client.Close();

                }


        }

        private bool IsValidJson(string strInput)
        {
            if (String.IsNullOrEmpty(strInput) == false)
            {
                strInput = strInput.Trim();
                if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                    (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
                {
                    try
                    {
                        var obj = JToken.Parse(strInput);
                        return true;
                    }
                    catch (JsonReaderException jex)
                    {
                        //Exception in parsing json
                        Console.WriteLine(jex.Message);
                        return false;
                    }
                    catch (Exception ex) //some other exception
                    {

                        Console.WriteLine(ex.ToString());
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            
        }

        public bool IsDateUnixTime(string date)
        {
            try
            {
                int i = Int32.Parse(date);
                return true;
            }
            catch (Exception e)
            {
               
                return false;
            }
          
            
        }

        public bool isValidPath(string path, string method)
        {
            bool valid = false;

            if (String.IsNullOrEmpty(path) == false && path.Contains("/"))
            {
                string[] strList = path.Split("/");

                bool checkPathID()
                {
                    try
                    {
                        int i = Int32.Parse(strList[3]);
                        return true;
                    }
                    catch (Exception)
                    {

                        return false;
                    }
                }               
                if (strList[1].Equals("api"))
                {                    
                    valid = true;

                    if (strList.ElementAtOrDefault(2) != null && strList[2].Equals("categories"))
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = false;
                    }
                    if (strList.ElementAtOrDefault(3) != null && checkPathID() == true)
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = false;
                    }
                    if (strList.ElementAtOrDefault(1) != null && strList.ElementAtOrDefault(2) != null && strList[1].Equals("api") && strList[2].Equals("categories"))
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = false;
                    }

                    if (strList.ElementAtOrDefault(3) == null)
                    {
                        if (method=="create")
                        {
                            valid= true;
                        }
                    if (method=="update" || method == "delete")
                        {
                            valid = false;
                        }
                        return valid;
                    }
                    else
                    {
                        if (checkPathID())
                        {
                            if (method == "read" || method == "update" || method == "delete")
                            {
                                valid = true;
                            }
                            else
                            {
                                valid = false;
                            }
                        }
                        else
                        {
                            valid = false;
                        }
                        

                         return valid;
                }

                }
                else
                {
                    Console.WriteLine("path is not valid");
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        public void SendResponse(Response responseObject, TcpClient client)
        {
            Console.WriteLine("This is response: " + responseObject.ToString());
            string responseString = System.Text.Json.JsonSerializer.Serialize(responseObject, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var responseBytes = Encoding.UTF8.GetBytes(responseString);
            client.GetStream().Write(responseBytes, 0, responseBytes.Length);
        }

    }



    }
    

