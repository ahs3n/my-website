var mobileCheck = {
    IsMobile: function()
    {
        console.log("Mobile check complete");
        return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    }
};
 
mergeInto(LibraryManager.library, mobileCheck);