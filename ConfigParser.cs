using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using CPDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using CPSection = System.Collections.Generic.Dictionary<string, string>;

public class ConfigParser
{
	
    /// <summary>
    /// Config Datas.
    /// </summary>
	private CPDictionary _Dictionary = null;

    /// <summary>
    /// Constractor.
    /// </summary>
	public ConfigParser()
	{
        this._Dictionary = new CPDictionary();
	}

    /// <summary>
    /// Get SectionDataset in [Default] section.
    /// </summary>
    /// <returns></returns>
	public CPSection defaults()
	{
		return _Dictionary["Default"];
	}

    /// <summary>
    /// Read init file style text file.
    /// </summary>
    /// <param name="pFilePath"> File path to the file. Can use relative path. </param>
    /// <returns> Return 'true' if Succeeded. </returns>
	public bool read(string pFilePath)
	{
        string lFPath = System.IO.Path.GetFullPath(pFilePath);
        if (!File.Exists(lFPath)) {
            throw new FileNotFoundException();
		}
        var lFp = File.Open(lFPath, System.IO.FileMode.Open);
        bool result = this.readfp( lFp );
        return result;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pFileStream"></param>
    /// <returns></returns>
	public bool readfp( FileStream pFileStream )
	{
		string lCurrentSection = "Default";
		char[] lTrimmer = { '[', ']' };
		char[] lSplitter = { ':', '=' };

		var lReader = new StreamReader( pFileStream );
		while( lReader.Peek() > -1 ) {
			string lLine = lReader.ReadLine();

			//----- Skip blank line -----//
			if( lLine == "" ) continue;

			//----- New Section -----//
			if( Regex.IsMatch( lLine, @"\[.*\]" ) ) {
				lLine = lLine.Trim( lTrimmer );
				this.add_section( lLine );
				lCurrentSection = lLine;
				continue;
			}

			//----- Add Option -----//
			if( lLine.Contains( "=" ) || lLine.Contains( ":" ) ) {

				var lSplits = lLine.Split( lSplitter );
				var lOption = lSplits.GetValue( 0 ).ToString().Trim();
				var lValue = lSplits.GetValue( 1 ).ToString().Trim();
				this.set( lCurrentSection, lOption, lValue );
                continue;
			}

		}

		return true;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pSection"></param>
    /// <returns></returns>
	public bool has_section( string pSection )
	{
        if (this._Dictionary.ContainsKey(pSection)) {
            return true;
        }
		return false;
	}

    /// <summary>
    /// Add new section to config data.
    /// </summary>
    /// <param name="pSectionName"> Section name. </param>
    ///
    void test() {}

    /// <summary>
    /// Add new section to config data.
    /// </summary>
    /// <param name="pSectionName"> Section name. </param>
    /// <returns> If the section is already exist, return false. </returns>
	public bool add_section( string pSectionName )
	{
        if (this.has_section(pSectionName)) return false;
		var lDict = this._Dictionary;
		var lNewSetions = new CPSection();
		lDict.Add( pSectionName, lNewSetions );
        return true;
	}

    /// <summary>
    /// Set value to the field.
    /// </summary>
    /// <param name="pSection"> Section name to set value. </param>
    /// <param name="pOption"> Option name to set value. </param>
    /// <param name="pValue"> Value(string) to set. </param>
	public void set( string pSection, string pOption, string pValue )
	{
		if( !this.has_section(pSection)) {
			this.add_section(pSection);
		}
		var lSection = this._Dictionary[pSection];
		lSection[pOption] = pValue;
	}

    /// <summary>
    /// Get array of section names.
    /// </summary>
    /// <returns> String array. </returns>
	public string[] sections()
	{
		return this._Dictionary.Keys.ToArray();
	}

    /// <summary>
    /// Get dictionary data in section.
    /// </summary>
    /// <param name="pSection">Section name.</param>
    /// <returns> Dictionary data contains option and value pair. </returns>
	public CPSection items( string pSection )
	{
		if( this.has_section(pSection)) {
			return this._Dictionary[pSection];
		}
		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="lSection"> Section name </param>
	/// <returns></returns>
	public string[] options( string lSection )
	{
		if( !this.has_section( lSection ) ) {
			return new string[0];
		}
		return this._Dictionary[lSection].Keys.ToArray();
	}

    /// <summary>
    /// Check config data already has the option.
    /// </summary>
    /// <param name="pSection"> Section name. </param>
    /// <param name="pOption"> option name </param>
    /// <returns> If already exist, return true. </returns>
	public bool has_option( string pSection, string pOption )
	{
		CPDictionary lDict = this._Dictionary;
		if( !this.has_section( pSection ) ) {
			return false;
		}
		if( !lDict.ContainsKey( pOption ) ) {
			return false;
		}
		return true;
	}

    /// <summary>
    /// Get the option value.
    /// </summary>
    /// <param name="pSection"> Section name. </param>
    /// <param name="pOption"> Option name. </param>
    /// <returns> If section or option is not exist, return null. </returns>
	public string get( string pSection, string pOption ){
		if( !this._Dictionary.ContainsKey( pSection ) ) {
			return null;
		}
		var lSection = this.items(pSection);
		if( !lSection.ContainsKey( pOption ) ) {
			return null;
		}
		return lSection[pOption];
	}

    /// <summary>
    /// Export Config File.
    /// </summary>
    /// <param name="pFilePath"> File path to export. </param>
    /// <param name="pMakeDirectory"> If true, create new directory when export directory is not exist. Else, throw exception. </param>
    /// <returns> Path to exported file. </returns>
	public string write( string pFilePath, bool pMakeDirectory=true )
	{
        //----- Check directory exist -----//
        string lFPath = System.IO.Path.GetFullPath(pFilePath);
        var lDir = System.IO.Path.GetDirectoryName(lFPath);
        if (!System.IO.Directory.Exists(lDir)) {
            if (pMakeDirectory) {
                System.IO.Directory.CreateDirectory(lDir);
            } else {
                throw new DirectoryNotFoundException(lDir);
            }
        }
        //----- Write file -----//
        var lFp = File.Open(lFPath, System.IO.FileMode.OpenOrCreate);
        var result = this.writefp(lFp);
        lFp.Close();
        return lFPath;
	}

    /// <summary>
    /// Write Config File to file stream.
    /// </summary>
    /// <param name="pFileStream"></param>
    /// <returns></returns>
    public bool writefp( FileStream pFileStream )
    {
        var lWriter = new StreamWriter(pFileStream);
        string[] lSections = this.sections();
        foreach (string lSection in lSections) {
            //----- Section name -----//
            lWriter.WriteLine(string.Format("[{0}]", lSection));

            string[] lOptions = this.options(lSection);
            foreach (string lOption in lOptions) {
                var lValue = this.get(lSection, lOption);
                lWriter.WriteLine(string.Format("{0} = {1}", lOption, lValue));
            }

        }
        lWriter.Close();
        return true;
    }

}
