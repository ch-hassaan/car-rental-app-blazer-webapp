using Microsoft.EntityFrameworkCore; 
using RentalBlazorApp.Data; 
using RentalBlazorApp.Models; 

namespace RentalBlazorApp.Services;


public class CarService
{
    
    
    private readonly IDbContextFactory<AppDbContext> _db;

    
    public CarService(IDbContextFactory<AppDbContext> db) => _db = db;

    
    public async Task<List<Car>> GetAllAsync()
    {
        using var ctx = _db.CreateDbContext(); 
        
        return await ctx.Cars.OrderBy(c => c.Category).ThenBy(c => c.Name).ToListAsync();
    }

    
    public async Task<List<Car>> GetByCategoryAsync(string cat)
    {
        using var ctx = _db.CreateDbContext();
        
        return await ctx.Cars.Where(c => c.Category.ToLower() == cat.ToLower()).OrderBy(c => c.Name).ToListAsync();
    }

    
    public async Task<Car?> GetByIdAsync(string id)
    {
        using var ctx = _db.CreateDbContext();
        return await ctx.Cars.FindAsync(id); 
    }

    
    public async Task AddAsync(Car car)
    {
        using var ctx = _db.CreateDbContext();
        ctx.Cars.Add(car); 
        await ctx.SaveChangesAsync(); 
    }

    
    public async Task UpdateAsync(Car car)
    {
        using var ctx = _db.CreateDbContext();
        ctx.Cars.Update(car); 
        await ctx.SaveChangesAsync();
    }

    
    public async Task DeleteAsync(string id)
    {
        using var ctx = _db.CreateDbContext();
        var car = await ctx.Cars.FindAsync(id); 
        if (car != null) 
        { 
            ctx.Cars.Remove(car); 
            await ctx.SaveChangesAsync(); 
        }
    }

    
    public async Task UpdateStatusAsync(string id, CarStatus status)
    {
        using var ctx = _db.CreateDbContext();
        var car = await ctx.Cars.FindAsync(id);
        if (car != null) 
        { 
            car.Status = status; 
            await ctx.SaveChangesAsync(); 
        }
    }

    
    public async Task SeedCarsAsync()
    {
        using var ctx = _db.CreateDbContext();
        if (await ctx.Cars.AnyAsync()) return; 

        
        ctx.Cars.AddRange(new List<Car>
        {
            
            new(){Name="Audi A5",Category="Luxury Sedans",Price=220000,ImageUrl="https://audiapproved.com/_next/image?url=https%3A%2F%2Fadmin.audiapproved.com%2Fuploads%2Fcars%2F98962%2Fupload-12.jpg&w=3840&q=75",Seats="5",Fuel="Petrol",Transmission="Manual",Mileage="50,000 km",Condition="9.8/10"},
            new(){Name="Toyota Crown",Category="Luxury Sedans",Price=200000,ImageUrl="https://carsforsale.co.ke/wp-content/uploads/2024/01/2018-Toyota-Crown-RS-2.0-Turbo.jpg",Seats="5",Fuel="Petrol",Transmission="Automatic",Mileage="65,000 km",Condition="8/10"},
            new(){Name="Hyundai Elantra 2024",Category="Luxury Sedans",Price=75000,ImageUrl="https://vehicle-images.dealerinspire.com/ccfd-110005802/KMHLS4DG1RU700004/df09007020aceea59e2a36d6dfcf87e9.jpg",Seats="5",Fuel="Petrol",Transmission="Automatic",Mileage="43,000 km",Condition="8/10"},
            new(){Name="Honda Civic RS 2024",Category="Luxury Sedans",Price=64000,ImageUrl="https://silamparitv.disway.id/upload/3f06321284f1299de998045b2d4c3a85.jpg",Seats="5",Fuel="Petrol",Transmission="Automatic",Mileage="55,000 km",Condition="10/10"},
            new(){Name="Toyota Camry 2022",Category="Luxury Sedans",Price=45000,ImageUrl="https://paultan.org/image/2022/02/2022-Toyota-Camry-facelift-Malaysia-1.jpg",Seats="5",Fuel="Petrol",Transmission="Automatic",Mileage="77,000 km",Condition="9.5/10"},
            new(){Name="Hyundai Sonata 2023",Category="Luxury Sedans",Price=350000,ImageUrl="https://www.tulsahyundai.com/blogs/3678/wp-content/uploads/2022/11/2023-Hyundai-Sonata.jpeg",Seats="5",Fuel="Petrol",Transmission="Automatic",Mileage="33,000 km",Condition="9/10"},
            
            new(){Name="2024 BMW X7",Category="SUVs",Price=250000,ImageUrl="https://static0.carbuzzimages.com/wordpress/wp-content/uploads/2024/03/1099617-7.jpg",Seats="5",Fuel="Petrol",Transmission="8-speed automatic",Mileage="50,000 km",Condition="9.8/10"},
            new(){Name="Toyota Land Cruiser 300",Category="SUVs",Price=68000,ImageUrl="https://editorial.pxcrush.net/carsales/general/editorial/toyota-landcruiser-300-sahara-zx_0824.jpg?width=1024&height=682",Seats="5-7",Fuel="Diesel",Transmission="10-speed automatic",Mileage="75,599 km",Condition="9/10"},
            new(){Name="2024 Lexus GX 550",Category="SUVs",Price=80000,ImageUrl="https://imageio.forbes.com/specials-images/imageserve/66ccad0a50d7223893cec519/2024-Lexus-GX-550/960x0.png?format=png&width=960",Seats="6-7",Fuel="Petrol",Transmission="10-speed automatic",Mileage="55,300 km",Condition="9/10"},
            new(){Name="Range Rover Sport",Category="SUVs",Price=175000,ImageUrl="https://vehicle-images.dealerinspire.com/05e0-11001579/thumbnails/large/SAL1P9EU7RA414263/b3738a694e1036ae1043b4dd010e576d.jpg",Seats="5",Fuel="PHEV",Transmission="8-speed automatic",Mileage="100,000 km",Condition="9.5/10"},
            new(){Name="Mercedes-Benz GLE",Category="SUVs",Price=200000,ImageUrl="https://i.gaw.to/vehicles/photos/40/23/402337-2021-mercedes-benz-gle.jpg?1024x640",Seats="5-7",Fuel="Petrol",Transmission="9-speed automatic",Mileage="65,000 km",Condition="9.5/10"},
            new(){Name="Audi Q8",Category="SUVs",Price=150000,ImageUrl="https://cdn.motor1.com/images/mgl/mrY7q/s1/audi-q8-tfsi-e-quattro-2020.webp",Seats="5",Fuel="Petrol",Transmission="8-speed Automatic",Mileage="89,000 km",Condition="9/10"},
            
            new(){Name="Mazda MX-5 Miata",Category="Convertibles",Price=72000,ImageUrl="https://cdn.jdpower.com/JDPA_2020%20Mazda%20MX-5%20Miata%20RF%20Grand%20Touring%20Polymetal%20Gray%20Front%20View.jpg",Seats="2",Fuel="Petrol",Transmission="Manual",Mileage="50,000 km",Condition="9.8/10"},
            new(){Name="Benz E-Class Cabriolet",Category="Convertibles",Price=80000,ImageUrl="https://www.thecarexpert.co.uk/wp-content/uploads/2021/10/35661-NewE-ClassCabriolet-2133x1200-cropped.jpeg",Seats="4",Fuel="Petrol",Transmission="Automatic",Mileage="65,000 km",Condition="8/10"},
            new(){Name="Mercedes C-Class Cabriolet",Category="Convertibles",Price=95000,ImageUrl="https://www.carscoops.com/wp-content/uploads/2016/02/Mercedes-C-Class-Cabrio-1855.jpg",Seats="4",Fuel="Petrol",Transmission="Automatic",Mileage="43,000 km",Condition="8/10"},
            new(){Name="Mini Cooper S Convertible",Category="Convertibles",Price=750000,ImageUrl="https://www.mini.co.uk/en_GB/home/mini-news/convertible-seaside-edition/jcr:content/main/par/product_editorial_10/productEditorialPar/editorial_fullwidth__1716008769/leftPar/image_item/damImage.wide.1350w.j_1674060008963.jpg",Seats="4",Fuel="Petrol",Transmission="Manual",Mileage="43,000 km",Condition="9/10"},
            new(){Name="BMW 2 Series Convertible",Category="Convertibles",Price=55000,ImageUrl="https://parkers-images.bauersecure.com/wp-images/21739/cut-out/930x620/bmw_2_series_conv.jpg",Seats="4",Fuel="Petrol",Transmission="Automatic",Mileage="77,000 km",Condition="9.5/10"},
            new(){Name="Volkswagen Beetle Convertible",Category="Convertibles",Price=34000,ImageUrl="https://www.newbeetle.org/attachments/vw-front-left-top-down-jpg.53231/",Seats="4",Fuel="Petrol",Transmission="Automatic",Mileage="55,000 km",Condition="10/10"},
            
            new(){Name="BMW i4",Category="Electric",Price=53000,ImageUrl="https://images.carexpert.com.au/crop/1200/630/app/uploads/2024/01/BMW-i4-eDrive35_HERO-16x9-1.jpg",Seats="5",Fuel="Electric",Transmission="Automatic",Mileage="44,000 km",Condition="9.8/10"},
            new(){Name="BYD Seal",Category="Electric",Price=60000,ImageUrl="https://media.autoexpress.co.uk/image/private/s--X-WVjvBW--/f_auto,t_content-image-full-desktop@1/v1700047049/autoexpress/2023/11/BYD%20Seal%202023%20UK-13.jpg",Seats="5",Fuel="Electric",Transmission="Automatic",Mileage="77,900 km",Condition="8/10"},
            new(){Name="Porsche Taycan",Category="Electric",Price=70000,ImageUrl="https://di-uploads-pod15.dealerinspire.com/porschenortholmstedrafihautogroup/uploads/2024/04/2025-Porsche-Taycan.jpg",Seats="5",Fuel="Electric",Transmission="Automatic",Mileage="33,000 km",Condition="9.8/10"},
            new(){Name="Audi Etron",Category="Electric",Price=350000,ImageUrl="https://img.freepik.com/premium-photo/futuristic-audi-q4-electric-suv-modern-showroom-highquality-stock-image_1097779-9828.jpg",Seats="5",Fuel="Electric",Transmission="Automatic",Mileage="55,800 km",Condition="8/10"},
            new(){Name="KIA EV6",Category="Electric",Price=45000,ImageUrl="https://www.electrive.com/media/2024/05/kia-ev6-2024-scaled-e1715677872698.jpg",Seats="5",Fuel="Electric",Transmission="Automatic",Mileage="44,000 km",Condition="9/10"},
            new(){Name="Cadillac Lyriq",Category="Electric",Price=54000,ImageUrl="https://di-uploads-pod30.dealerinspire.com/jerryseinercadillac/uploads/2023/03/mlp-img-top-2024-lyriq-temp.jpg",Seats="5",Fuel="Electric",Transmission="Automatic",Mileage="66,000 km",Condition="7.8/10"},
            
            new(){Name="Rolls-Royce Wraith 2016",Category="Opulence",Price=470000,ImageUrl="https://images.turo.com/media/vehicle/images/7H5XTHOGQHu6dZL4uvYL6g.1242x745.jpg",Seats="4",Fuel="Petrol",Transmission="Automatic",Mileage="23,000 km",Condition="10/10"},
            new(){Name="Bentley Flying Spur 2021",Category="Opulence",Price=345500,ImageUrl="https://hips.hearstapps.com/hmg-prod/images/2021-bentley-flying-spur-mmp-1-1595611033.jpg",Seats="5",Fuel="Petrol",Transmission="Automatic",Mileage="18,500 km",Condition="10/10"},
            new(){Name="Mercedes-Benz S-Class 2021",Category="Opulence",Price=216000,ImageUrl="https://th.bing.com/th?id=OIP.jAzrBEn-KSmZBAvqVXNO3wHaEK&w=333&h=187&c=8&rs=1&qlt=90&o=6&dpr=1.5&pid=3.1&rm=2",Seats="5",Fuel="Petrol",Transmission="9-speed automatic",Mileage="32,000 km",Condition="10/10"},
            new(){Name="Porsche Panamera 2021",Category="Opulence",Price=180000,ImageUrl="https://th.bing.com/th/id/OIP.qyqU6VAs1jssxxCKXuX99wHaFj?w=244&h=184&c=7&r=0&o=5&dpr=1.5&pid=1.7",Seats="4",Fuel="Petrol",Transmission="Dual-clutch auto",Mileage="43,000 km",Condition="10/10"},
            new(){Name="Maserati Quattroporte",Category="Opulence",Price=150000,ImageUrl="https://th.bing.com/th/id/OIP.-90AgQGpi5GET0GGjj0gywHaEK?w=259&h=180&c=7&r=0&o=5&dpr=1.5&pid=1.7",Seats="4",Fuel="Petrol",Transmission="Automatic",Mileage="62,000 km",Condition="9.5/10"},
            new(){Name="Audi A8L",Category="Opulence",Price=200000,ImageUrl="https://th.bing.com/th/id/OIP.5KNbXKQ5nDUlwWBylaj9iQHaE8?w=251&h=180&c=7&r=0&o=5&dpr=1.5&pid=1.7",Seats="4",Fuel="Petrol",Transmission="Automatic",Mileage="38,000 km",Condition="9/10"},
            
            new(){Name="KIA Grand Carnival",Category="Minivans",Price=33000,ImageUrl="https://www.bolnews.com/wp-content/uploads/2024/01/FotoJet-17.jpg",Seats="11",Fuel="Petrol",Transmission="Manual",Mileage="44,000 km",Condition="9.7/10"},
            new(){Name="Hyundai Staria",Category="Minivans",Price=30000,ImageUrl="https://hyundaikpk.com/wp-content/uploads/2023/10/staria_exterior2.jpg",Seats="7-9",Fuel="Diesel",Transmission="8-Speed Auto",Mileage="55,300 km",Condition="8/10"},
            new(){Name="Toyota Hiace Luxury",Category="Minivans",Price=20000,ImageUrl="https://www.toyota-central.com/Assets/images/Vehicle/HiaceDeluxe/Color/Color-Range.png",Seats="10",Fuel="Diesel",Transmission="6-speed Auto",Mileage="68,000 km",Condition="10/10"},
            new(){Name="Honda Odyssey",Category="Minivans",Price=350000,ImageUrl="https://www.postcrescent.com/gcdn/-mm-/d8ae6c35729db557fea21a1c14e320417a5481ed/c=0-306-1800-1321/local/-/media/WIGroup/Appleton/2014/08/12/1407872595000-2014-Honda-Odyssey-minivan.jpg",Seats="8",Fuel="Petrol",Transmission="10-speed Auto",Mileage="99,200 km",Condition="9/10"},
            new(){Name="Mercedes-Benz V-Class",Category="Minivans",Price=45000,ImageUrl="https://i.ytimg.com/vi/NWH1FDWEgeI/hq720.jpg",Seats="7",Fuel="Petrol",Transmission="Automatic",Mileage="88,000 km",Condition="9/10"},
            new(){Name="Suzuki APV",Category="Minivans",Price=24000,ImageUrl="https://upload.wikimedia.org/wikipedia/commons/3/32/2014_Suzuki_APV_Arena_SGX_1.5_DN42V_%2820190623%29.jpg",Seats="7",Fuel="Petrol",Transmission="Manual",Mileage="59,000 km",Condition="8/10"},
            
            new(){Name="Mercedes-Benz A-Class",Category="Hatchbacks",Price=50000,ImageUrl="https://cdn.motor1.com/images/mgl/pmbRW/s1/4x3/2018-mercedes-benz-a-class.webp",Seats="5",Fuel="Petrol",Transmission="7-speed dual-clutch",Mileage="55,000 km",Condition="9.8/10"},
            new(){Name="Audi A5 Sportback",Category="Hatchbacks",Price=90000,ImageUrl="https://hips.hearstapps.com/hmg-prod/images/2025-audi-a5-137-669583e0eda6e.jpg",Seats="5",Fuel="Gasoline",Transmission="Automatic",Mileage="75,599 km",Condition="9/10"},
            new(){Name="BMW 4 Series Coupe",Category="Hatchbacks",Price=100000,ImageUrl="https://www.automoblog.com/wp-content/uploads/2021/06/2022-BMW-4-Series-Gran-Coupe-1.jpg",Seats="4",Fuel="Gasoline",Transmission="8-speed automatic",Mileage="89,300 km",Condition="9.2/10"},
            new(){Name="Porche 718 Cayman",Category="Hatchbacks",Price=150000,ImageUrl="https://pictures.porsche.com/rtt/iris?COSY-EU-100-1711coMvsi60AAt5FwcmBEgA4qP8iBUDxPE3Cb9pNXkBuNYdMGF4tl3U0%25z8rMHIspbWvanYb%255y%25oq%25vSTmjMXD4qAZeoNBPUSfUx4RmHlCgI7Zl2dioCx3hQDcFG8UpYnfurEU65yPewymeCvNzxMYHGXoq1kGUr6FObzWSwRuT0qyzx7e2HXWv1UzQK7e%25bsqYSg35yPewQ9eCvNzxFsOGXVD2UxLODmLRXi978gTfeIJpV7nDhQh",Seats="2",Fuel="Gasoline",Transmission="Automatic",Mileage="100,000 km",Condition="9.5/10"},
            new(){Name="Mercedes-Benz EQS Sedan",Category="Hatchbacks",Price=300000,ImageUrl="https://i.ytimg.com/vi/XF6yXf0e70E/hq720.jpg",Seats="5",Fuel="Electric",Transmission="Single-speed auto",Mileage="45,000 km",Condition="10/10"},
            new(){Name="Audi S7",Category="Hatchbacks",Price=150000,ImageUrl="https://carsguide-res.cloudinary.com/image/upload/f_auto,fl_lossy,q_auto,t_default/v1/editorial/review/hero_image/2020-Audi-S7-Sportback-Sedan-White-1001x565%20(3).jpg",Seats="5",Fuel="Gasoline",Transmission="Automatic",Mileage="60,000 km",Condition="9.5/10"},
            
            new(){Name="BMW X5",Category="Crossovers",Price=35000,ImageUrl="https://images.prismic.io/carwow/585d8660-215c-4396-95a9-267473baa569_2018+BMW+X5+Front+3%3A4+Driving+1.jpg",Seats="5",Fuel="Petrol / Diesel",Transmission="8-speed automatic",Mileage="75,000 km",Condition="9.8/10"},
            new(){Name="Porsche Macan Turbo",Category="Crossovers",Price=30000,ImageUrl="https://ev-database.org/img/auto/Porsche_Macan_Turbo_2024/Porsche_Macan_Turbo_2024-01@2x.jpg",Seats="5",Fuel="Petrol",Transmission="7-speed dual clutch",Mileage="48,990 km",Condition="9.5/10"},
            new(){Name="Range Rover Velar",Category="Crossovers",Price=45000,ImageUrl="https://akm-img-a-in.tosshub.com/businesstoday/images/story/202309/whatsapp_image_2023-09-15_at_12-sixteen_nine.jpeg",Seats="5",Fuel="Petrol / Diesel",Transmission="8-speed automatic",Mileage="150,000 km",Condition="9.2/10"},
            new(){Name="Lexus RX 350",Category="Crossovers",Price=28000,ImageUrl="https://www.metrolexus.com/static/dealer-17081/videos/2311-Lexus-RX.gif",Seats="5",Fuel="Petrol",Transmission="8-speed automatic",Mileage="78,000 km",Condition="9/10"},
            new(){Name="Audi Q5",Category="Crossovers",Price=20000,ImageUrl="https://cms.motorcomplete.co.uk/media/zngguoja/q5-1.jpg?width=1200&height=630&quality=90&mode=crop&scale=both&center=0,0",Seats="5",Fuel="Petrol",Transmission="7-speed dual clutch",Mileage="65,000 km",Condition="9.5/10"},
            new(){Name="Mercedes-Benz GLC",Category="Crossovers",Price=32000,ImageUrl="https://media.ed.edmunds-media.com/mercedes-benz/glc-class-coupe/2025/oem/2025_mercedes-benz_glc-class-coupe_4dr-suv_amg-glc-43_fq_oem_1_1600.jpg",Seats="5",Fuel="Petrol / Diesel",Transmission="9-speed automatic",Mileage="99,700 km",Condition="9/10"},
            
            new(){Name="Yamaha YZF-R1",Category="Super Bikes",Price=10300,ImageUrl="https://cdn.bikedekho.com/processedimages/yamaha/yamaha-yzf-r1/source/yamaha-yzf-r166ec16c2ecaa7.jpg",Seats="2",Fuel="Petrol",Transmission="6-speed",Mileage="998 cc",Condition="9/10"},
            new(){Name="Ducati Panigale",Category="Super Bikes",Price=36000,ImageUrl="https://cdn.bikedekho.com/processedimages/ducati/panigale-v4/source/panigale-v46756c46e25a7a.jpg",Seats="2",Fuel="Petrol",Transmission="6-speed",Mileage="955 cc",Condition="9/10"},
            new(){Name="Suzuki GSX-R600",Category="Super Bikes",Price=18000,ImageUrl="https://cache4.pakwheels.com/ad_pictures/8080/suzuki-gsx-r600-2017-80806029.webp",Seats="2",Fuel="Petrol",Transmission="6-speed",Mileage="599 cc",Condition="9/10"},
            new(){Name="Honda CBR-500R",Category="Super Bikes",Price=28000,ImageUrl="https://i.pinimg.com/736x/01/38/d4/0138d4b65cb90f6d41e2d0a93b89fdb4.jpg",Seats="2",Fuel="Petrol",Transmission="6-speed",Mileage="471 cc",Condition="9/10"},
            new(){Name="Suzuki Hayabusa",Category="Super Bikes",Price=25000,ImageUrl="https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hayabusa.jpg/640px-Hayabusa.jpg",Seats="2",Fuel="Petrol",Transmission="6-speed",Mileage="1340 cc",Condition="9/10"},
            new(){Name="BMW S1000RR",Category="Super Bikes",Price=24000,ImageUrl="https://cdn.bikedekho.com/processedimages/bmw/s1000rr/640X309/s1000rr63944bf4cf2d5.jpg",Seats="2",Fuel="Petrol",Transmission="5-speed",Mileage="883 cc",Condition="9/10"},
            
            new(){Name="Harley-Davidson Street Glide",Category="Chopper Bikes",Price=30000,ImageUrl="https://www.thunderbike.com/wp-content/uploads/2020/11/4k-BaggerMichael-Rauscher111.jpg",Seats="2",Fuel="Petrol",Transmission="6-speed MT",Mileage="1,746 cc",Condition="9.8/10"},
            new(){Name="Harley-Davidson Forty-Eight",Category="Chopper Bikes",Price=20000,ImageUrl="https://i0.wp.com/roadbikemag.com/wp-content/uploads/2021/04/Harley-Davidson-Forty-Eight.png?fit=1200%2C800&ssl=1",Seats="2",Fuel="Petrol",Transmission="5-speed MT",Mileage="1,202 cc",Condition="9/10"},
            new(){Name="Indian Scout",Category="Chopper Bikes",Price=15000,ImageUrl="https://www.usatoday.com/gcdn/-mm-/100b55026b69b9ab05750bb7c661ca173038e565/c=0-58-700-453/local/-/media/2018/02/14/USATODAY/USATODAY/636542241805922595-Indian.leftview01.jpg",Seats="2",Fuel="Petrol",Transmission="5-speed MT",Mileage="1,133 cc",Condition="9.2/10"},
            new(){Name="Harley-Davidson Iron 883",Category="Chopper Bikes",Price=25000,ImageUrl="https://sklmotodubai.com/wp-content/uploads/2024/05/20240509_181608.jpg",Seats="2",Fuel="Petrol",Transmission="5-speed MT",Mileage="883 cc",Condition="9/10"},
            new(){Name="Harley-Davidson V-Rod",Category="Chopper Bikes",Price=35000,ImageUrl="https://res.cloudinary.com/italtour/image/upload/c_limit,f_auto,g_center,h_455,q_auto,w_780/v1491591102/ukh6lcqh9snjtnzep1mt.jpg",Seats="2",Fuel="Petrol",Transmission="5-speed MT",Mileage="1,247 cc",Condition="9/10"},
            new(){Name="Suzuki Intruder",Category="Chopper Bikes",Price=18000,ImageUrl="https://moto.motorionline.com/wp-content/uploads/2012/07/suzuki-intruder-la-passione-per-il-custom-intruder-m1800r.jpg",Seats="2",Fuel="Petrol",Transmission="5-speed MT",Mileage="1,783 cc",Condition="9.7/10"},
        });
        await ctx.SaveChangesAsync(); 
    }
}
