CREATE OR REPLACE FUNCTION fn_set_modified_datetime()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.modified_datetime = NOW();
    RETURN NEW;
END;
$$;