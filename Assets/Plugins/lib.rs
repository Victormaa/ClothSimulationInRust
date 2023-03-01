#[no_mangle]
pub extern "C" fn add(left: i32, right: i32) -> i32 {
    left + right
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub struct Vector3 {
    pub x: f32,
    pub y: f32,
    pub z: f32,
}

impl Vector3 {
    pub fn new(x:f32, y:f32, z:f32) -> Self{
        Self { x: x, y: y, z: z }
    }
    pub fn zero() -> Self{
        Self { x: 0.0, y: 0.0, z: 0.0 }
    }
    pub fn magnitude(&self) -> f32 {
        (self.x * self.x + self.y * self.y + self.z * self.z).sqrt()
    }
    pub fn normalized(&self) -> Self{
        let length = self.magnitude();
        Self { x: self.x/length,
               y: self.y/length,
               z: self.z/length, }
    }
    pub fn sub(&self, subber : &Vector3) -> Self{
        Self { x: self.x - subber.x,
               y: self.y - subber.y,
               z: self.z - subber.z, }
    }
    pub fn mul_f32(&self, muller : f32) -> Self{
        Vector3 { x: self.x * muller,
             y: self.y * muller, 
             z: self.z * muller }
    }
}

#[no_mangle]
pub extern "C" fn is_inside_sphere_collider(point: Vector3, sphere_pos: Vector3, sphere_radius: f32) -> Vector3{
    let mut force_normal : Vector3 = Vector3::zero();

    let p_to_s = point.sub(&sphere_pos);

    if p_to_s.magnitude() < sphere_radius {
        force_normal = p_to_s.normalized();
    }

    force_normal
}

#[no_mangle]
pub extern "C" fn gravity_force(gravity : f32) -> Vector3{
    Vector3::new(0.0, -gravity, 0.0)
}

#[no_mangle]
pub extern "C" fn wind_force(point: Vector3, 
    wind_min_x:f32, wind_max_x : f32, wind_min_y : f32, wind_max_y : f32,
    wind_force : f32) -> Vector3{

        if point.x > wind_min_x 
        && point.x < wind_max_x
        && point.y > wind_min_y
        && point.y < wind_max_y{
            Vector3::new(0.0, 0.0, wind_force)
        }else{
            Vector3::zero()
        }
    }
    
    #[no_mangle]
    pub extern "C" fn inner_force(point_1 : Vector3, point_2 : Vector3, k : f32, l : f32) -> Vector3{
        let p2_to_p1 = point_2.sub(&point_1);

        let f = p2_to_p1.normalized().mul_f32(-k).mul_f32(l - p2_to_p1.magnitude());

        f
    }

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn it_works() {
        let result = add(2, 2);
        assert_eq!(result, 4);
    }

    // #[test]
    // fn insideSphereWork(){
    //     let a = is_inside_sphere_collider((1.0,1.0,1.0), (1.0,1.0,1.0), 4.0);
    // }
}
