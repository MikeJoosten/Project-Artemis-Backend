﻿using Recollectable.Core.Entities.Collectables;
using Recollectable.Core.Entities.Locations;
using System;

namespace Recollectable.API.Models.Collectables
{
    public class CoinDto
    {
        public Guid Id { get; set; }
        public int FaceValue { get; set; }
        public string Type { get; set; }
        public string ReleaseDate { get; set; }
        public int Mintage { get; set; }
        public double Weight { get; set; }
        public double Size { get; set; }
        public string Metal { get; set; }
        public string Note { get; set; }
        public string Subject { get; set; }
        public string ObverseDescription { get; set; }
        public string ObverseInscription { get; set; }
        public string ObverseLegend { get; set; }
        public string ReverseDescription { get; set; }
        public string ReverseInscription { get; set; }
        public string ReverseLegend { get; set; }
        public string EdgeType { get; set; }
        public string EdgeLegend { get; set; }
        public string Designer { get; set; }
        public string HeadOfState { get; set; }
        public string MintMark { get; set; }
        public string FrontImagePath { get; set; }
        public string BackImagePath { get; set; }
        public Country Country { get; set; }
        public CollectorValue CollectorValue { get; set; }
    }
}