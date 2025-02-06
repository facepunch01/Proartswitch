use winit::{
    application::ApplicationHandler,
    event::WindowEvent,
    event_loop::{ActiveEventLoop, EventLoop},
};
use trayicon::{Icon, MenuBuilder, TrayIcon, TrayIconBuilder};
use winreg::enums::*;
use winreg::RegKey;
use std::error::Error;
#[derive(Clone, Eq, PartialEq, Debug)]
enum UserEvents {
    RightClickTrayIcon,
    LeftClickTrayIcon,
    Exit,
}

fn main() {
    let event_loop = EventLoop::<UserEvents>::with_user_event().build().unwrap();
    let proxy = event_loop.create_proxy();

    
    let icon = include_bytes!("../on.ico");
    let first_icon = Icon::from_buffer(icon, None, None).unwrap();
    let icon2 = include_bytes!("../off.ico");
    let second_icon = Icon::from_buffer(icon2, None, None).unwrap();

    let tray_icon = TrayIconBuilder::new()
        .sender(move |e: &UserEvents| {
            let _ = proxy.send_event(e.clone());
        })
        .icon_from_buffer(icon)
        .tooltip("Switch windows slate mode")
        .on_click(UserEvents::LeftClickTrayIcon)
        .on_right_click(UserEvents::RightClickTrayIcon)
        .menu(
            MenuBuilder::new()
                .item("E&xit", UserEvents::Exit),
        )
        .build()
        .unwrap();

    let mut app = MyApplication {
        tray_icon,
        first_icon,
        second_icon,
        is_on: true,
    };
    event_loop.run_app(&mut app).unwrap();
}
pub fn switch_state(on: bool) -> Result<(), Box<dyn Error>> {
    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let slate: RegKey = hklm.open_subkey_with_flags(r#"SYSTEM\\CurrentControlSet\\Control\\PriorityControl"#, KEY_WRITE)?;
    if on {
        slate.set_value("ConvertibleSlateMode", &1u32)?;
    }
    else {
        slate.set_value("ConvertibleSlateMode", &0u32)?;
    }
    Ok(())
}
struct MyApplication {
    tray_icon: TrayIcon<UserEvents>,
    first_icon: Icon,
    second_icon: Icon,
    is_on: bool,
}

impl ApplicationHandler<UserEvents> for MyApplication {
    fn resumed(&mut self, _event_loop: &ActiveEventLoop) {}

    // Platform specific events
    fn window_event(
        &mut self,
        event_loop: &ActiveEventLoop,
        _window_id: winit::window::WindowId,
        event: WindowEvent,
    ) {
        match event {
            WindowEvent::CloseRequested => {
                event_loop.exit();
            }
            _ => {}
        }
    }

    // Application specific events
    fn user_event(&mut self, event_loop: &ActiveEventLoop, event: UserEvents) -> () {
        match event {
            UserEvents::Exit => event_loop.exit(),
            UserEvents::RightClickTrayIcon => {
                self.tray_icon.show_menu().unwrap();
            }
            UserEvents::LeftClickTrayIcon => {
                switch_state(self.is_on).expect("Reg write error");
                self.is_on = !self.is_on;
                if self.is_on { self.tray_icon.set_icon(&self.second_icon).unwrap(); } // Probably a better way to do this
                else { self.tray_icon.set_icon(&self.first_icon).unwrap(); }
            }
        }
    }
}